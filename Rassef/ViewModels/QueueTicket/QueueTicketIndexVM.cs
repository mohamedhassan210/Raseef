namespace Rassef.ViewModels.QueueTicket
{
    public class LiveDockCardVM
    {
        public string DockLetter { get; set; } = "A";
        public string DockName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string CurrentTicketNumber { get; set; } = "—";
        public string NextTicketNumber { get; set; } = "—";
        public string Status { get; set; } = "إنتظار";
    }

    public class QueueTicketIndexVM
    {
        public List<QueueTicketListVM> Tickets { get; set; } = new();

        public List<LiveDockCardVM> DockCards { get; set; } = new();

        public int CompletedCount { get; set; }

        public int InProgressCount { get; set; }

        public int WaitingCount { get; set; }
    }
}