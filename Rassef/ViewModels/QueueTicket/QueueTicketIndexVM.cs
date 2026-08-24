namespace Rassef.ViewModels.QueueTicket
{
    public class LiveDockCardVM
    {
        public string DockLetter { get; set; } = "A";
        public string DockName { get; set; } = "رصيف 5";
        public string CurrentTicketNumber { get; set; } = "A265";
        public string NextTicketNumber { get; set; } = "A266";
        public string Status { get; set; } = "جاري";
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