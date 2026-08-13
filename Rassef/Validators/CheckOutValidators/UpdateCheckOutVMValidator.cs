namespace Rassef.Validators.CheckOutValidators
{
    public class UpdateCheckOutVMValidator : AbstractValidator<UpdateCheckOutVM>
    {
        public UpdateCheckOutVMValidator()
        {
            RuleFor(x => x.TicketId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.ExitTypeId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
