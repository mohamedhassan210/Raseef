namespace Rassef.ViewModels.QueueTicket
{
    public class QueueTicketIndexVM
    {
        public List<QueueTicketListVM> Tickets { get; set; } = new();

        public int CompletedCount { get; set; }

        public int InProgressCount { get; set; }

        public int WaitingCount { get; set; }
    }
}