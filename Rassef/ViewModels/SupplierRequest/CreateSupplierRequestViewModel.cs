public class CreateSupplierRequestVM
{
    [Required(ErrorMessage = "برجاء اختيار المورد")]
    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار الشاحنة")]
    [Display(Name = "الشاحنة")]
    public int TruckId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار السائق")]
    [Display(Name = "السائق")]
    public int DriverId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار القسم")]
    [Display(Name = "القسم")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار نوع التصريح")]
    [Display(Name = "نوع التصريح")]
    public int PermitTypeId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار نوع السلعة")]
    [Display(Name = "نوع السلعة")]
    public int CommodityTypeId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار حالة الطلب")]
    [Display(Name = "حالة الطلب")]
    public int RequestStatusId { get; set; }

    [Display(Name = "صورة بطاقة السائق")]
    public string DriverNationalCardPhoto { get; set; } = string.Empty;

    [Required(ErrorMessage = "برجاء إدخال رقم هاتف السائق")]
    [Display(Name = "رقم هاتف السائق")]
    public string DriverPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "برجاء إدخال رقم التصريح")]
    [Display(Name = "رقم التصريح")]
    public string PermitNumber { get; set; } = string.Empty;

    [Display(Name = "مواد غذائية؟")]
    public bool IsFood { get; set; }

    // قوائم الاختيارات (Dropdowns) للقوالب
    public IEnumerable<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Trucks { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Drivers { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PermitTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CommodityTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> RequestStatuses { get; set; } = new List<SelectListItem>();
}