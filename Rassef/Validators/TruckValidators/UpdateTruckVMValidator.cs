namespace Rassef.Validators.TruckValidators
{
    public class UpdateTruckVMValidator : AbstractValidator<UpdateTruckVM>
    {
        public UpdateTruckVMValidator()
        {
            RuleFor(x => x.PlateNumber).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.PlateLetter).NotEmpty().WithMessage("الحقل مطلوب");
        }
    }
}
