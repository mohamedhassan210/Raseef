namespace Rassef.Validators.ShiftValidators
{
    public class UpdateShiftVMValidator : AbstractValidator<UpdateShiftVM>
    {
        public UpdateShiftVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
        }
    }
}
