using FluentValidation;

namespace Rassef.Validators.AuthenticationValidators
{
    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(x=>x.UserNameOrEmail).NotEmpty().WithMessage("Please confirm your password. ");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Passowrd is required. ")
                .MinimumLength(8).WithMessage("Password must be at least 6 characters.") ;

        }
    }
}
