namespace Rassef.Validators.PermitTypesValidators
{
    public class CreatePermitTypesValidator : AbstractValidator<CreatePermitTypesVM>
    {
        public CreatePermitTypesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع التصريح.")
                .MaximumLength(100).WithMessage("اسم نوع التصريح يجب ألا يتجاوز 100 حرف.");
        }
    }
}
