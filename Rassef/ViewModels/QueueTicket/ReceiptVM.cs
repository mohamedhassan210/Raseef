namespace Rassef.ViewModels.QueueTicket
{
    public class ReceiptVM
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = "توريد";
        public string DepartmentName { get; set; } = string.Empty;
        public string DockName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string WaitingCount { get; set; } = "0";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    }
}
