namespace Rassef.Validators.DockAssignmentValidators
{
    public class UpdateDockAssignmentVMValidator : AbstractValidator<UpdateDockAssignmentVM>
    {
        public UpdateDockAssignmentVMValidator()
        {
            RuleFor(x => x.DockId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
            RuleFor(x => x.TicketId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
