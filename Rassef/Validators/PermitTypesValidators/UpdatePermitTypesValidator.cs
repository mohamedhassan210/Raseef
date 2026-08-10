namespace Rassef.Validators.PermitTypesValidators
{
    public class UpdatePermitTypesValidator : AbstractValidator<UpdatePermitTypesVM>
    {
        public UpdatePermitTypesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع التصريح غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع التصريح.")
                .MaximumLength(100).WithMessage("اسم نوع التصريح يجب ألا يتجاوز 100 حرف.");
        }
    }
}
