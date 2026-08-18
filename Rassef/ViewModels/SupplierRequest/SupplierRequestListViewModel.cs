public class SupplierRequestListVM
{
    public int Id { get; set; }

    [Display(Name = "نوع الطلب")]
    public string RequestType { get; set; } = "توريد";

    [Display(Name = "المورد")]
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "رقم لوحة الشاحنة")]
    public string TruckPlateNumber { get; set; } = string.Empty;

    [Display(Name = "اسم السائق")]
    public string DriverName { get; set; } = string.Empty;
    [Display(Name = "الموظف")]
    public string EmployeeName { get; set; } = string.Empty;
    [Display(Name = "رقم هاتف السائق")]
    public string DriverPhone { get; set; } = string.Empty;
    [Display(Name = "رقم التذكرة")]
    public string TicketNumber { get; set; } = string.Empty;

    [Display(Name = "القسم")]
    public string DepartmentName { get; set; } = string.Empty;

    [Display(Name = "حالة الطلب")]
    public string RequestStatusName { get; set; } = string.Empty;

    [Display(Name = "رقم التصريح")]
    public string PermitNumber { get; set; } = string.Empty;
    public string TicketStatusName { get; set; } = string.Empty;
    public int DockNumber { get; set; }
    [Display(Name = "الرصيف")]
    public string DockName { get; set; } = string.Empty;
    public DateTimeOffset QueueTime { get; set; }

}