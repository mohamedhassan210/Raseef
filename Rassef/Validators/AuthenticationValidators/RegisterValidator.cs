
namespace Rassef.Validators.AuthenticationValidators
{
    public class RegisterValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
                .Length(3, 50).WithMessage("يجب أن يكون طول اسم المستخدم بين 3 و 50 حرفاً.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
                .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم الكامل 100 حرف.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .MaximumLength(100).WithMessage("يجب ألا يتجاوز البريد الإلكتروني 100 حرف.")
                .EmailAddress().WithMessage("يرجى إدخال بريد إلكتروني صحيح.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .Length(8, 50).WithMessage("يجب أن تكون كلمة المرور بين 8 و 50 حرفاً.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("يرجى تأكيد كلمة المرور.")
                .Equal(x => x.Password).WithMessage("كلمتا المرور غير متطابقتين.");
        }
    }
}