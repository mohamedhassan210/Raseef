
namespace Rassef.Validators.QueueTicket
{
    public class CreateQueueTicketValidator : AbstractValidator<CreateQueueTicketVM>
    {
        public CreateQueueTicketValidator()
        {
            RuleFor(x => x.TicketNumber)
                .NotEmpty().WithMessage("رقم التذكرة مطلوب.")
                .MaximumLength(50).WithMessage("رقم التذكرة يجب ألا يتجاوز 50 حرفًا.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("يرجى اختيار القسم بشكل صحيح.");

            RuleFor(x => x.TicketStatusId)
                .GreaterThan(0).WithMessage("يرجى اختيار حالة التذكرة بشكل صحيح.");

            When(x => x.TransferRequestId.HasValue, () =>
            {
                RuleFor(x => x.TransferRequestId!.Value)
                    .GreaterThan(0).WithMessage("طلب التحويل غير صالحة.");
            });

            When(x => x.SupplierRequestId.HasValue, () =>
            {
                RuleFor(x => x.SupplierRequestId!.Value)
                    .GreaterThan(0).WithMessage("طلب المورد غير صالحة.");
            });

            RuleFor(x => x.EntryTime)
                .GreaterThanOrEqualTo(x => x.QueueTime)
                .WithMessage("وقت الدخول لا يمكن أن يكون قبل وقت الانتظار.");

            RuleFor(x => x.ExitTime)
                .GreaterThanOrEqualTo(x => x.EntryTime)
                .WithMessage("وقت الخروج لا يمكن أن يكون قبل وقت الدخول.");
        }
    }
}
