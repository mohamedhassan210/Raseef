namespace Rassef.Validators.QueueActionValidators
{
    public class CreateQueueActionVMValidator : AbstractValidator<CreateQueueActionVM>
    {
        public CreateQueueActionVMValidator()
        {
            RuleFor(x => x.ActionTypeId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
