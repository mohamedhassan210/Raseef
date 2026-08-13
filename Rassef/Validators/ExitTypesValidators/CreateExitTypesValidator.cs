namespace Rassef.Validators.ExitTypesValidators
{
    public class CreateExitTypesValidator : AbstractValidator<CreateExitTypesVM>
    {
        public CreateExitTypesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم نوع الخروج.")
                .MaximumLength(100).WithMessage("اسم نوع الخروج يجب ألا يتجاوز 100 حرف.");
        }
    }
}
