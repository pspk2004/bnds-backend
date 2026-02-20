using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0.");

        RuleFor(x => x.BidEndTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Bid end time must be in the future.");
    }
}

public class CreateBidValidator : AbstractValidator<CreateBidDto>
{
    public CreateBidValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Bid amount must be greater than 0.");
    }
}

public class PurchaseMembershipValidator : AbstractValidator<PurchaseMembershipDto>
{
    public PurchaseMembershipValidator()
    {
        RuleFor(x => x.MembershipId).GreaterThan(0).WithMessage("Membership selection is required.");
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(x => x == "Card" || x == "UPI").WithMessage("Payment method must be 'Card' or 'UPI'.");
    }
}

public class SuspendUserValidator : AbstractValidator<SuspendUserDto>
{
    public SuspendUserValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SuspensionDays).GreaterThan(0).WithMessage("Suspension days must be greater than 0.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.");
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(500);
    }
}

public class PayPenaltyValidator : AbstractValidator<PayPenaltyDto>
{
    public PayPenaltyValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(x => x == "Card" || x == "UPI").WithMessage("Payment method must be 'Card' or 'UPI'.");
    }
}
