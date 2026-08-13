namespace Rassef.Validators.DepartmentTypeValidators
{
    public class UpdateDepartmentTypesValidator : AbstractValidator<UpdateDepartmentTypesVM>
    {
        public UpdateDepartmentTypesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع القسم غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع القسم.")
                .MaximumLength(100).WithMessage("اسم نوع القسم يجب ألا يتجاوز 100 حرف.");
        }
    }
}
