namespace Rassef.ViewModels.Supplier
{
    public class CreateSupplierVM
    {
        [Required(ErrorMessage = "اسم المورد مطلوب.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "يجب أن يكون اسم المورد بين 2 و 100 حرف.")]
        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب.")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف غير صحيح.")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "شعار المورد (Logo)")]
        public IFormFile? LogoFile { get; set; }
    }
}