namespace Rassef.Validators.DockValidators
{
    public class CreateDockVMValidator : AbstractValidator<CreateDockVM>
    {
        public CreateDockVMValidator()
        {
            RuleFor(x => x.DockName).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.DockName).MaximumLength(100);
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
