public class UpdateSupplierRequestVM
{
    public int Id { get; set; }

    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Display(Name = "الشاحنة")]
    public int TruckId { get; set; }

    [Display(Name = "السائق")]
    public int DriverId { get; set; }

    [Display(Name = "القسم")]
    public int DepartmentId { get; set; }

    [Display(Name = "نوع التصريح")]
    public int PermitTypeId { get; set; }

    [Display(Name = "نوع السلعة")]
    public int CommodityTypeId { get; set; }

    [Display(Name = "حالة الطلب")]
    public int RequestStatusId { get; set; }

    [Display(Name = "صورة بطاقة السائق")]
    public string DriverNationalCardPhoto { get; set; } = string.Empty;

    [Display(Name = "رقم هاتف السائق")]
    public string DriverPhone { get; set; } = string.Empty;

    [Display(Name = "رقم التصريح")]
    public string PermitNumber { get; set; } = string.Empty;

    [Display(Name = "مواد غذائية؟")]
    public bool IsFood { get; set; }

    //   (Dropdowns) 
    public IEnumerable<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Trucks { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Drivers { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PermitTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CommodityTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> RequestStatuses { get; set; } = new List<SelectListItem>();
}