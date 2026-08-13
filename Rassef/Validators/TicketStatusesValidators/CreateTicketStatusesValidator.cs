namespace Rassef.Validators.TicketStatusesValidators
{
    public class CreateTicketStatusesValidator : AbstractValidator<CreateTicketStatusesVM>
    {
        public CreateTicketStatusesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("يرجى إدخال اسم حالة التذكرة.")
                .MaximumLength(100).WithMessage("اسم حالة التذكرة يجب ألا يتجاوز 100 حرف.");
        }
    }
}
