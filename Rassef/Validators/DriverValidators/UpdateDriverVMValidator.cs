namespace Rassef.Validators.DriverValidators
{
    public class UpdateDriverVMValidator : AbstractValidator<UpdateDriverVM>
    {
        public UpdateDriverVMValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.FullName).MaximumLength(100);
            RuleFor(x => x.NationalId).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.NationalId).Length(14).Matches(@"^\d{14}$").WithMessage("الرقم القومي غير صحيح");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Phone).Matches(@"^01[0-2,5]{1}[0-9]{8}$").WithMessage("رقم الهاتف غير صحيح.");
        }
    }
}
