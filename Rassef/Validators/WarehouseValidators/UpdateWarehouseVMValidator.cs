namespace Rassef.Validators.WarehouseValidators
{
    public class UpdateWarehouseVMValidator : AbstractValidator<UpdateWarehouseVM>
    {
        public UpdateWarehouseVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.Location).NotEmpty().WithMessage("الحقل مطلوب");
        }
    }
}
