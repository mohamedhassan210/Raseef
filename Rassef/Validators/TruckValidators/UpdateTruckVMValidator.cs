using FluentValidation;
using Rassef.ViewModels.Truck;

namespace Rassef.Validators.TruckValidators
{
    public class UpdateTruckVMValidator : AbstractValidator<UpdateTruckVM>
    {
        public UpdateTruckVMValidator()
        {
            RuleFor(x => x.TruckNumber)
                .NotEmpty().WithMessage("رقم الشاحنة مطلوب");

            RuleFor(x => x.TruckLetters)
                .NotEmpty().WithMessage("حروف الشاحنة مطلوبة");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("سعة التخزين يجب أن تكون أكبر من الصفر");
        }
    }
}