using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");

            entity.HasOne(u => u.Membership)
                  .WithMany(m => m.Users)
                  .HasForeignKey(u => u.MembershipId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000).IsRequired();
            entity.Property(p => p.StartingPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CurrentPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Seller)
                  .WithMany(u => u.Products)
                  .HasForeignKey(p => p.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Winner)
                  .WithMany(u => u.WonProducts)
                  .HasForeignKey(p => p.WinnerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Bid
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(b => b.Product)
                  .WithMany(p => p.Bids)
                  .HasForeignKey(b => b.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bids)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Membership
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).HasMaxLength(50).IsRequired();
            entity.Property(m => m.Price).HasColumnType("decimal(18,2)");
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Type).HasMaxLength(50).IsRequired();
            entity.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(t => t.PaymentMethod).HasMaxLength(20).IsRequired();

            entity.HasOne(t => t.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
            entity.Property(n => n.Message).HasMaxLength(1000).IsRequired();

            entity.HasOne(n => n.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
