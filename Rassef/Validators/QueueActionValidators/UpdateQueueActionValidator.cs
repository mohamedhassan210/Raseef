namespace Rassef.Validators.QueueActionValidators
{


    public class UpdateQueueActionValidator : AbstractValidator<UpdateQueueActionVM>
    {
        public UpdateQueueActionValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("المعرف (ID) مطلوب.");

            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("يرجى اختيار تذكرة صالحة.");

            RuleFor(x => x.ActionTypeId)
                .GreaterThan(0).WithMessage("يرجى اختيار نوع الإجراء.");

            RuleFor(x => x.ActionTime)
                .NotEmpty().WithMessage("وقت الإجراء مطلوب.");
        }
    }
}