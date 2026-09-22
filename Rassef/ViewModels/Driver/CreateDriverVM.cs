namespace Rassef.ViewModels.Driver
{
    public class CreateDriverVM
    {
        [Display(Name = "اسم السائق")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "المورد")]
        public int? SupplierId { get; set; }


        [Display(Name = "اسم الشركة")]
        public string? SupplierName { get; set; }

        [Display(Name = "رقم الشاحنة")]
        public int? TruckId { get; set; }

        // القسم والرصيف اللي المستخدم اختارهم في مودال "اختار القسم" —
        // بيتمروا لصفحة إنشاء الشاحنة عشان يبقوا مُختارين مسبقاً هناك
        public int? DepartmentId { get; set; }
        public int? DockId { get; set; }


        public IEnumerable<SelectListItem> Suppliers { get; set; }
            = new HashSet<SelectListItem>();
    }
}