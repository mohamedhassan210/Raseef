namespace Rassef.ViewModels.QueueTicket
{
    public class CallStationVM
    {
        public QueueTicketListVM? CurrentTicket { get; set; }
        public QueueTicketListVM? NextUpcomingTicket { get; set; }
        public List<QueueTicketListVM> WaitingQueue { get; set; } = new List<QueueTicketListVM>();
        public int WaitingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedTodayCount { get; set; }
        public int? SelectedDepartmentId { get; set; }
    }
}
