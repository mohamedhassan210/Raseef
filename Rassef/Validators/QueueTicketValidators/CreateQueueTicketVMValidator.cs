namespace Rassef.Validators.QueueTicketValidators
{
    public class CreateQueueTicketVMValidator : AbstractValidator<CreateQueueTicketVM>
    {
        public CreateQueueTicketVMValidator()
        {
            RuleFor(x => x.TicketNumber).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.TicketStatusId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
