namespace Rassef.Validators.DriverTypeValidators
{
    public class CreateDriverTypeValidator : AbstractValidator<CreateDriverTypeVM>
    {
        public CreateDriverTypeValidator()
        {
            RuleFor(x => x.Code)
                .GreaterThan(0).When(x => x.Code.HasValue)
                .WithMessage("كود نوع السائق يجب ألا يكون بالسالب أو صفراً.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع السائق.")
                .MaximumLength(100).WithMessage("اسم نوع السائق يجب ألا يتجاوز 100 حرف.");
        }
    }
}
