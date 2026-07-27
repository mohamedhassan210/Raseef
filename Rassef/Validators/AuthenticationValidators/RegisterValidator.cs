

using FluentValidation;

namespace Rassef.Validators.AuthenticationValidators
{
    public class RegisterValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("User Name is required.")
                .Length(3, 50);

            RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                .MaximumLength(100)
                .EmailAddress();

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.")
                .Length(8, 50);

            RuleFor(x => x.ConfirmPassword)
                            .NotEmpty().WithMessage("Please confirm your password.")
                            .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
