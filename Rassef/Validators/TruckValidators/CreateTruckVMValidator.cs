namespace Rassef.Validators.TruckValidators
{
    public class CreateTruckVMValidator : AbstractValidator<CreateTruckVM>
    {
        public CreateTruckVMValidator()
        {
            RuleFor(x => x.PlateNumber).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.PlateLetter).NotEmpty().WithMessage("الحقل مطلوب");
        }
    }
}
