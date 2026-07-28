
namespace Rassef.ViewModels.Supplier
{
    public class SupplierDetailsVM
    {
        public Guid Id { get; set; }

        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "شعار المورد")]
        public string? LogoURL { get; set; }

        [Display(Name = "تم الإنشائ بواسطة")]
        public string? CreatedByUserName { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "عدد الطلبات الخاصه بالمورد")]
        public int TotalRequestsCount { get; set; }
    }
}