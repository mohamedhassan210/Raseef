namespace Rassef.Validators.DepartmentTypeValidators
{
    public class CreateDepartmentTypesValidator : AbstractValidator<CreateDepartmentTypesVM>
    {
        public CreateDepartmentTypesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع القسم.")
                .MaximumLength(100).WithMessage("اسم نوع القسم يجب ألا يتجاوز 100 حرف.");
        }
    }
}
