
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
                .MinimumLength(6).WithMessage("يجب ألا تقل كلمة المرور عن 6 أحرف.");
        }
    }
}