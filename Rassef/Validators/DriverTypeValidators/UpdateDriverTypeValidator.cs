namespace Rassef.Validators.DriverTypeValidators
{
    public class UpdateDriverTypeValidator : AbstractValidator<UpdateDriverTypeVM>
    {
        public UpdateDriverTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع السائق غير صالح.");

            RuleFor(x => x.Code)
                .GreaterThan(0).When(x => x.Code.HasValue)
                .WithMessage("كود نوع السائق يجب ألا يكون بالسالب أو صفراً.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع السائق.")
                .MaximumLength(100).WithMessage("اسم نوع السائق يجب ألا يتجاوز 100 حرف.");
        }
    }
}
