using FluentValidation;
using Rassef.ViewModels.Administration;

namespace Rassef.Validators.DriverValidators
{
    /// <summary>
    /// Validator for the Administration DriverCreateVM
    /// </summary>
    public class DriverCreateAdminVMValidator : AbstractValidator<DriverCreateVM>
    {
        public DriverCreateAdminVMValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("اسم السائق مطلوب")
                .MaximumLength(100).WithMessage("الاسم لا يجب أن يتجاوز 100 حرف");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب")
                .Matches(@"^(010|011|012|015)\d{8}$")
                .WithMessage("رقم الهاتف غير صالح (يجب أن يكون رقم مصري مكون من 11 رقم)");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب")
                .Length(14).WithMessage("الرقم القومي يجب أن يكون 14 رقماً بالضبط")
                .Matches(@"^\d{14}$").WithMessage("الرقم القومي يجب أن يحتوي على أرقام فقط");

            RuleFor(x => x.DeiverTypeId)
                .GreaterThan(0).WithMessage("يرجى اختيار نوع السائق");
        }
    }
}
