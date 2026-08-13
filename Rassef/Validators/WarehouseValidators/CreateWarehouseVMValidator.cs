namespace Rassef.Validators.WarehouseValidators
{
    public class CreateWarehouseVMValidator : AbstractValidator<CreateWarehouseVM>
    {
        public CreateWarehouseVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.Location).NotEmpty().WithMessage("الحقل مطلوب");
        }
    }
}
