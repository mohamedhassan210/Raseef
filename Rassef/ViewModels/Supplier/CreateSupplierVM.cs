namespace Rassef.ViewModels.Supplier
{
    public class CreateSupplierVM
    {
        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "كود المورد")]
        public string SupCode { get; set; } = string.Empty;
        [Display(Name = "شعار المورد (Logo)")]

        public IFormFile? LogoFile { get; set; }
    }
}