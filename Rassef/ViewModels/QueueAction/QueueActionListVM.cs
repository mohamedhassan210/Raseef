namespace Rassef.ViewModels.QueueAction
{
    public class QueueActionListVM
    {
        public int Id { get; set; }

        [Display(Name = "رقم التذكرة")]
        public int TicketId { get; set; }

        [Display(Name = "نوع الإجراء")]
        public string ActionTypeName { get; set; } = string.Empty;

        [Display(Name = "وقت الإجراء")]
        public DateTimeOffset ActionTime { get; set; }
    }
}