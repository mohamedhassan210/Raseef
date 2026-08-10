
namespace Rassef.Validators.Driver
{
    public class UpdateDriverValidator : AbstractValidator<UpdateDriverVM>
    {
        public UpdateDriverValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف السائق غير صالح.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("اسم السائق مطلوب.")
                .MinimumLength(3).WithMessage("اسم السائق يجب أن يكون 3 أشكال/حروف على الأقل.")
                .MaximumLength(100).WithMessage("اسم السائق يجب ألا يتجاوز 100 حرف.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح يتكون من 11 رقم ويبرأ بـ 01.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.")
                .Matches(@"^[23]\d{13}$").WithMessage("الرقم القومي يجب أن يتكون من 14 رقم ويبرأ بـ 2 أو 3.");
        }
    }
}