using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Membership> _membershipRepo;
    private readonly IConfiguration _config;

    public AuthService(IRepository<User> userRepo, IRepository<Membership> membershipRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _membershipRepo = membershipRepo;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userRepo.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        var freeMembership = await _membershipRepo.FirstOrDefaultAsync(m => m.Name == "Free");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            MembershipId = freeMembership?.Id,
            MembershipExpiry = DateTime.UtcNow.AddMonths(freeMembership?.DurationMonths ?? 6),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            UserId = user.Id
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password.");

        if (user.IsSuspended && user.SuspensionEndDate.HasValue && user.SuspensionEndDate > DateTime.UtcNow)
            throw new InvalidOperationException($"Your account is suspended until {user.SuspensionEndDate:yyyy-MM-dd}. Pay the penalty to lift suspension.");

        // Auto-lift expired suspensions
        if (user.IsSuspended && user.SuspensionEndDate.HasValue && user.SuspensionEndDate <= DateTime.UtcNow)
        {
            user.IsSuspended = false;
            user.SuspensionEndDate = null;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            UserId = user.Id
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
