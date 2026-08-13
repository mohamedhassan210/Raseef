namespace Rassef.ViewModels.SupplyOrderReport
{
    public class SupplyOrderReportViewModel
    {
        public int RequestId { get; set; }
        public string QueueNumber { get; set; } = string.Empty;     // TicketNumber من QueueTicket
        public string DriverName { get; set; } = string.Empty;      // FullName من Driver
        public string TruckPlate { get; set; } = string.Empty;       // PlateLetter + PlateNumber من Truck
        public string DockName { get; set; } = string.Empty;        // رقم الرصيف
        public string DepartmentName { get; set; } = string.Empty;  // Name من Department
        public DateTimeOffset EntryTime { get; set; }               // EntryTime أو QueueTime من QueueTicket
        public string StatusName { get; set; } = string.Empty;      // Name من RequestStatuses
    }
}
