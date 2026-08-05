namespace Rassef.Validators.ShiftValidators
{
    public class CreateShiftVMValidator : AbstractValidator<CreateShiftVM>
    {
        public CreateShiftVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
        }
    }
}
