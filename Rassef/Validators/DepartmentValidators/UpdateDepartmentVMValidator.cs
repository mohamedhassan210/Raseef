namespace Rassef.Validators.DepartmentValidators
{
    public class UpdateDepartmentVMValidator : AbstractValidator<UpdateDepartmentVM>
    {
        public UpdateDepartmentVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.Prefix).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Prefix).MaximumLength(5).WithMessage("الاختصار لا يجب أن يتجاوز 5 أحرف");
        }
    }
}
