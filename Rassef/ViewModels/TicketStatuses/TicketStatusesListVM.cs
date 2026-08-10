namespace Rassef.ViewModels.TicketStatuses
{
    public class TicketStatusesListVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم حالة التذكرة")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد التذاكر المرتبطة")]
        public int QueueTicketsCount { get; set; }
    }
}
