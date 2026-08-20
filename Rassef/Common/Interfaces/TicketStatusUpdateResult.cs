namespace Rassef.Common.Interfaces
{
    public class TicketStatusUpdateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string TruckPlate { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DockName { get; set; } = string.Empty;
    }
}
