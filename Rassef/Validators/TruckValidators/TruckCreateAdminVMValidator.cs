using FluentValidation;
using Rassef.ViewModels.Administration;

namespace Rassef.Validators.TruckValidators
{
    /// <summary>
    /// Validator for the Administration TruckCreateVM
    /// </summary>
    public class TruckCreateAdminVMValidator : AbstractValidator<TruckCreateVM>
    {
        public TruckCreateAdminVMValidator()
        {
            RuleFor(x => x.PlateNumber)
                .NotEmpty().WithMessage("رقم اللوحة مطلوب")
                .Matches(@"^\d+$").WithMessage("رقم اللوحة يجب أن يحتوي على أرقام فقط");

            RuleFor(x => x.PlateLetter)
                .NotEmpty().WithMessage("حروف اللوحة مطلوبة")
                .Matches(@"^[^\d]+$").WithMessage("حروف اللوحة يجب أن تحتوي على حروف فقط بدون أرقام");

            RuleFor(x => x.StorageCapacity)
                .GreaterThan(0).WithMessage("سعة التخزين يجب أن تكون أكبر من صفر");

            RuleFor(x => x.TruckTypeId)
                .GreaterThan(0).WithMessage("يرجى اختيار فئة الشاحنة");
        }
    }
}
