namespace Rassef.Validators.DriverValidators
{
    public class CreateDriverVMValidator : AbstractValidator<CreateDriverVM>
    {
        public CreateDriverVMValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.FullName).MaximumLength(100);
            RuleFor(x => x.NationalId).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.NationalId).Length(14).Matches(@"^\d{14}$").WithMessage("الرقم القومي غير صحيح");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Phone)
                         .MaximumLength(11).WithMessage("يجب ألا يتجاوز رقم الهاتف 11 رقماً.")
                         .When(x => !string.IsNullOrEmpty(x.Phone))
                         .Matches(@"^01[0-2,5]{1}[0-9]{8}$").WithMessage("رقم الهاتف غير صحيح.");
            RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("يرجى الاختيار من القائمة");
        }
    }
}
