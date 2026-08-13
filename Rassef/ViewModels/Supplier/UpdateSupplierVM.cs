
namespace Rassef.ViewModels.Supplier
{
    public class UpdateSupplierVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "كود المورد مطلوب")]
        [Display(Name = "كود المورد")]
        public string SupCode { get; set; } = string.Empty;
        public string? ExistingLogoURL { get; set; }

        [Display(Name = "تغيير شعار المورد (اختياري)")]
        public IFormFile? LogoFile { get; set; }
    }
}