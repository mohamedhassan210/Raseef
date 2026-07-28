

namespace Rassef.Validators.SupplierValidators
{
    public class CreateSupplierValidator : AbstractValidator<CreateSupplierVM>
    {
        private readonly ISupplierRepository _supplierRepository;

        public CreateSupplierValidator(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المورد مطلوب.")
                .Length(2, 100).WithMessage("يجب أن يكون اسم المورد بين 2 و 100 حرف.")
                .MustAsync((name, _) => _supplierRepository.IsNameUniqueAsync(name))
                .WithMessage("اسم المورد مسجل بالفعل.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^01[0125][0-9]{8}$").WithMessage("رقم الهاتف غير صحيح.")
                .MustAsync((phone, _) => _supplierRepository.IsPhoneUniqueAsync(phone))
                .WithMessage("رقم الهاتف مسجل بالفعل لمورد آخر.");

            When(x => x.LogoFile != null, () =>
            {
                RuleFor(x => x.LogoFile!.Length)
                    .LessThanOrEqualTo(2 * 1024 * 1024).WithMessage("حجم الصورة يجب ألا يتجاوز 2 ميجابايت.");

                RuleFor(x => x.LogoFile!.FileName)
                    .Must(fileName => IsValidImageExtension(fileName))
                    .WithMessage("يرجى اختيار صورة بصيغة صحيحة (.jpg, .jpeg, .png, .webp).");
            });
        }

        private bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}