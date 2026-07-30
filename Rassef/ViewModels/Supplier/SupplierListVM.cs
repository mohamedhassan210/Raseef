
namespace Rassef.ViewModels.Supplier
{
    public class SupplierListVM
    {
        public int Id { get; set; }

        [Display(Name = "شعار المورد")]
        public string? LogoURL { get; set; }

        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "عدد الطلبات")]
        public int RequestsCount { get; set; }
    }
}