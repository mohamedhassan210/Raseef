namespace Rassef.Validators.DockStatuses
{
    public class CreateDockStatusesVMValidator : AbstractValidator<CreateDockStatusesVM>
    {
        public CreateDockStatusesVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة الرصيف.")
                .MaximumLength(100).WithMessage("اسم حالة الرصيف يجب ألا يتجاوز 100 حرف.");
        }
    }
}
