namespace Rassef.Validators.TicketStatusesValidators
{
    public class UpdateTicketStatusesValidator : AbstractValidator<UpdateTicketStatusesVM>
    {
        public UpdateTicketStatusesValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف حالة التذكرة غير صالح.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة التذكرة.")
                .MaximumLength(100).WithMessage("اسم حالة التذكرة يجب ألا يتجاوز 100 حرف.");
        }
    }
}
