

namespace Rassef.Validators.AuthenticationValidators
{
    public class ForgetPasswordValidator : AbstractValidator<ForgetPasswordViewModel>
    {
        public ForgetPasswordValidator()
        {
            RuleFor(x => x.Email).NotNull().WithMessage("البريد الإلكتروني مطلوب.");
            RuleFor(x => x.Email.Value)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .MaximumLength(100)
                .EmailAddress().WithMessage("يرجى إدخال بريد إلكتروني صحيح.")
                .When(x => x.Email != null);
        }
    }
}