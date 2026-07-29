public class SupplierRequestDetailsVM
{
    public int Id { get; set; }

    [Display(Name = "المورد")]
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "الشاحنة")]
    public string TruckInfo { get; set; } = string.Empty;

    [Display(Name = "السائق")]
    public string DriverName { get; set; } = string.Empty;

    [Display(Name = "رقم هاتف السائق")]
    public string DriverPhone { get; set; } = string.Empty;

    [Display(Name = "صورة بطاقة السائق")]
    public string DriverNationalCardPhoto { get; set; } = string.Empty;

    [Display(Name = "القسم")]
    public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "نوع التصريح")]
    public string PermitTypeName { get; set; } = string.Empty;

    [Display(Name = "رقم التصريح")]
    public string PermitNumber { get; set; } = string.Empty;

    [Display(Name = "نوع السلعة")]
    public string CommodityTypeName { get; set; } = string.Empty;

    [Display(Name = "حالة الطلب")]
    public string RequestStatusName { get; set; } = string.Empty;

    [Display(Name = "مواد غذائية؟")]
    public bool IsFood { get; set; }

    [Display(Name = "تم الإنشاء بواسطة")]
    public string CreatedByName { get; set; } = string.Empty;
}