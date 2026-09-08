namespace Rassef.Validators.TruckTypesValidators
{
    public class UpdateTruckTypesValidator : AbstractValidator<UpdateTruckTypesVM>
    {
        public UpdateTruckTypesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع الشاحنة غير صالح.");

            RuleFor(x => x.TruckTypeCode)
                .GreaterThan(0).WithMessage("كود نوع الشاحنة يجب ألا يكون بالسالب أو صفراً.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع الشاحنة.")
                .MaximumLength(100).WithMessage("اسم نوع الشاحنة يجب ألا يتجاوز 100 حرف.");
        }
    }
}