namespace Rassef.Validators.AuthenticationValidators
{
    public class UserProfileValidator : AbstractValidator<UserProfileViewModel>
    {
        public UserProfileValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم 100 حرف.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
                .Length(3, 50).WithMessage("يجب أن يكون طول اسم المستخدم بين 3 و 50 حرفاً.");

            RuleFor(x => x.Email)
                .NotNull().WithMessage("البريد الإلكتروني مطلوب.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Email.Value)
                        .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                        .EmailAddress().WithMessage("يرجى إدخال بريد إلكتروني صحيح.");
                });

            RuleFor(x => x.Phone)
                  .NotEmpty().WithMessage("برجاء إدخال رقم هاتف السائق.")
                  .Matches(@"^(010|011|012|015)\d{8}$")
                  .WithMessage("رقم هاتف السائق غير صالحة (يجب أن يكون رقم مصري مكون من 11 رقم).");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.")
                .Matches(@"^\d{14}$").WithMessage("يجب أن يتكون الرقم القومي من 14 رقماً بالضبط.");
        }
    }
}