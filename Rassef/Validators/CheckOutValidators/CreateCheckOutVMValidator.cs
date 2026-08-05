namespace Rassef.Validators.CheckOutValidators
{
    public class CreateCheckOutVMValidator : AbstractValidator<CreateCheckOutVM>
    {
        public CreateCheckOutVMValidator()
        {
            RuleFor(x => x.TicketId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.ExitTypeId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
