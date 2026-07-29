namespace Rassef.Validators.DockAssignmentValidators
{
    public class UpdateDockAssignmentValidator : AbstractValidator<UpdateDockAssignmentVM>
    {
        public UpdateDockAssignmentValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("المعرف (ID) غير صالح.");

            RuleFor(x => x.DockId)
                .GreaterThan(0).WithMessage("برجاء اختيار الرصيف بشكل صحيح.");

            RuleFor(x => x.TicketId)
                .GreaterThan(0).WithMessage("برجاء اختيار التذكرة بشكل صحيح.");

            RuleFor(x => x.AssignedAt)
                .NotEmpty().WithMessage("برجاء إدخال وقت التعيين.");

            RuleFor(x => x.FinishedAt)
                .NotEmpty().WithMessage("برجاء إدخال وقت الانتهاء.")
                .GreaterThan(x => x.AssignedAt)
                .WithMessage("وقت الانتهاء يجب أن يكون بعد وقت التعيين.");
        }
    }
}