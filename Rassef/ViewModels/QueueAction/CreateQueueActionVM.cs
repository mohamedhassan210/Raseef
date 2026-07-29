

namespace Rassef.ViewModels.QueueAction
{
    public class CreateQueueActionVM
    {
        [Display(Name = "التذكرة")]
        public int TicketId { get; set; }

        public IEnumerable<SelectListItem>? Tickets { get; set; }

        [Display(Name = "نوع الإجراء")]
        public int ActionTypeId { get; set; }

        public IEnumerable<SelectListItem>? ActionTypes { get; set; }

        [Display(Name = "وقت الإجراء")]
        public DateTimeOffset ActionTime { get; set; } = DateTimeOffset.Now;
    }
}