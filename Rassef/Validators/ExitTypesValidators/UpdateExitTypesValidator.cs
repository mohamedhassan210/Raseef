namespace Rassef.Validators.ExitTypesValidators
{
    public class UpdateExitTypesValidator : AbstractValidator<UpdateExitTypesVM>
    {
        public UpdateExitTypesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف نوع الخروج غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع الخروج.")
                .MaximumLength(100).WithMessage("اسم نوع الخروج يجب ألا يتجاوز 100 حرف.");
        }
    }
}
