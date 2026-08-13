namespace Rassef.Validators.DockAssignmentValidators
{
    public class CreateDockAssignmentVMValidator : AbstractValidator<CreateDockAssignmentVM>
    {
        public CreateDockAssignmentVMValidator()
        {
            RuleFor(x => x.DockId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.TicketId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
