namespace Rassef.Validators.ActionType
{
    public class CreateActionTypeValidator : AbstractValidator<CreateActionTypesVM>
    {
        public CreateActionTypeValidator()
        {
            RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("يرجى إدخال اسم الإجراء.")
                    .MaximumLength(100).WithMessage("اسم الإجراء يجب ألا يتجاوز 100 حرف.");
        }
    }
}