namespace Rassef.Validators.TruckTypesValidators
{
    public class CreateTruckTypesValidator : AbstractValidator<CreateTruckTypesVM>
    {
        public CreateTruckTypesValidator()
        {
            RuleFor(x => x.TruckTypeCode)
                .GreaterThan(0).WithMessage("كود نوع الشاحنة يجب ألا يكون بالسالب أو صفراً.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع الشاحنة.")
                .MaximumLength(100).WithMessage("اسم نوع الشاحنة يجب ألا يتجاوز 100 حرف.");
        }
    }
}