namespace Rassef.Validators.ActionType
{
    public class UpdateActionTypeValidator: AbstractValidator<UpdateActionTypesVM>
    {
        public UpdateActionTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف الإجراء غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم الإجراء.")
                .MaximumLength(100).WithMessage("اسم الإجراء يجب ألا يتجاوز 100 حرف.");
        }
    }
}
