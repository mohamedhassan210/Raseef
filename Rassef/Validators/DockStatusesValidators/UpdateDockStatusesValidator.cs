namespace Rassef.Validators.DockStatusesValidators
{
    public class UpdateDockStatusesVMValidator : AbstractValidator<UpdateDockStatusesVM>
    {
        public UpdateDockStatusesVMValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف حالة الرصيف غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة الرصيف.")
                .MaximumLength(100).WithMessage("اسم حالة الرصيف يجب ألا يتجاوز 100 حرف.");
        }
    }
}
