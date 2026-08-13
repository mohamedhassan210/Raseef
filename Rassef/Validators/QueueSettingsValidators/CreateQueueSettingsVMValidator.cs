namespace Rassef.Validators.QueueSettingsValidators
{
    public class CreateQueueSettingsVMValidator : AbstractValidator<CreateQueueSettingsVM>
    {
        public CreateQueueSettingsVMValidator()
        {
            RuleFor(x => x.ShiftId)
                .NotNull().When(x => x.ResetType == ResetType.ByShift).WithMessage("الشيفت مطلوب")
                .GreaterThan(0).When(x => x.ResetType == ResetType.ByShift).WithMessage("يرجى اختيار شيفت صحيح");
        }
    }
}
