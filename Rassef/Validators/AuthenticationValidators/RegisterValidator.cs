namespace Rassef.Validators.AuthenticationValidators
{
    public class RegisterValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.userName);
        }

    }
}
