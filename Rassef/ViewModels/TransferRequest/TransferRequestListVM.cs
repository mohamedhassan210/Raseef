public class TransferRequestListVM
{
    public int Id { get; set; }
    public string AvizNumber { get; set; } = string.Empty;
    public bool IsRefrigerated { get; set; }
    public bool IsFood { get; set; }
    public string Truck { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string TruckPlateNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int DockNumber { get; set; }
    public DateTime DateTime { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
}

  