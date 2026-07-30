
namespace Rassef.Validators.AuthenticationValidators
{
    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UserNameOrEmail)
                .NotEmpty().WithMessage("اسم المستخدم أو البريد الإلكتروني مطلوب.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(8).WithMessage("يجب ألا تقل كلمة المرور عن 8 أحرف.");
        }
    }
}