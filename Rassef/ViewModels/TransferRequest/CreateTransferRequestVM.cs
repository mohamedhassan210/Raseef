

namespace Rassef.ViewModels.TransferRequest
{
    public class CreateTransferRequestVM
    {
        // 1. تحديد القسم (Dropdown)
        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; } = Enumerable.Empty<SelectListItem>();

        // الرصيف — بيتفلتر حسب القسم المختار فوق (اختياري)
        [Display(Name = "الرصيف")]
        public int? DockId { get; set; }

        public IEnumerable<Rassef.ViewModels.Dock.DockOptionVM> AllDocks { get; set; } = new List<Rassef.ViewModels.Dock.DockOptionVM>();


        // 2. نوع الإذن (Radio Button: خروج / دخول)
        [Display(Name = "نوع الإذن")]
        public int PermitTypeId { get; set; }

        public IEnumerable<SelectListItem> PermitTypes { get; set; } = Enumerable.Empty<SelectListItem>();


        // 3. رقم الإذن (Input Text)
        [Display(Name = "رقم الإذن")]
        public string PermitNumber { get; set; } = string.Empty;


        // 4. رقم الأفيز (Input Text)
        [Display(Name = "رقم الأفيز")]
        public string AvizNumber { get; set; } = string.Empty;

        // 5. الشاحنة (Dropdown)
        [Display(Name = "الشاحنة")]
        public int TruckId { get; set; }
        public IEnumerable<SelectListItem> Trucks { get; set; } = Enumerable.Empty<SelectListItem>();

        // 6. السائق (Dropdown)
        [Display(Name = "السائق")]
        public int DriverId { get; set; }
        public IEnumerable<SelectListItem> Drivers { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}