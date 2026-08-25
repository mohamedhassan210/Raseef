namespace Rassef.Validators.SupplierValidators
{
    public class UpdateSupplierVMValidator : AbstractValidator<UpdateSupplierVM>
    {
        public UpdateSupplierVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المورد مطلوب")
                .MaximumLength(100).WithMessage("اسم المورد لا يجب أن يتجاوز 100 حرف");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("برجاء إدخال رقم هاتف المورد.")
                .Matches(@"^(010|011|012|015)\d{8}$")
                .WithMessage("رقم الهاتف غير صالح (يجب أن يكون رقم مصري مكون من 11 رقم).");

            RuleFor(x => x.SupCode)
                .NotEmpty().WithMessage("كود المورد مطلوب");
        }
    }
}
