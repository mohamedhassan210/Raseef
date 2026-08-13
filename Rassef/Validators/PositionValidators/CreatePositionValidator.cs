namespace Rassef.Validators.PositionValidators
{
    public class CreatePositionValidator : AbstractValidator<CreatePositionViewModel>
    {
        public CreatePositionValidator()
        {
            RuleFor(x => x.PositionCode)
                .GreaterThan(0).WithMessage("كود المنصب يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.PositionName)
                .NotEmpty().WithMessage("اسم المنصب مطلوب.")
                .Length(2, 100).WithMessage("اسم المنصب يجب أن يكون بين 2 و100 حرف.");
        }
    }
}