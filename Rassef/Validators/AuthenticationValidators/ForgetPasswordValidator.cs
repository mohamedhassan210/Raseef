namespace Rassef.Validators.AuthenticationValidators
{
    public class ForgetPasswordValidator :AbstractValidator<ForgetPasswordViewModel>
    {
        public ForgetPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
             .MaximumLength(100)
             .EmailAddress().WithMessage("please enter a valid email address");
        }
    }
}