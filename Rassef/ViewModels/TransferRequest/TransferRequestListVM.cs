public class TransferRequestListVM
{
    public int Id { get; set; }
    public string RequestType { get; set; } = "تحويل";
    public string AvizNumber { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string TruckPlateNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DockName { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
}

