namespace Rassef.Validators.SupplierValidators
{
    public class UpdateSupplierVMValidator : AbstractValidator<UpdateSupplierVM>
    {
        public UpdateSupplierVMValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().WithMessage("الحقل مطلوب");
            RuleFor(x => x.Phone)
                          .NotEmpty().WithMessage("برجاء إدخال رقم هاتف السائق.")
                          .Matches(@"^(010|011|012|015)\d{8}$")
                          .WithMessage("رقم هاتف السائق غير صالحة (يجب أن يكون رقم مصري مكون من 11 رقم)."); RuleFor(x => x.ExistingLogoURL).NotEmpty().WithMessage("الحقل مطلوب");
        }
    }
}
