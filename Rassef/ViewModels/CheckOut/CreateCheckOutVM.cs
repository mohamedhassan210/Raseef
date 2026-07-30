namespace Rassef.ViewModels.CheckOut
{
    public class CreateCheckOutVM
    {
        [Display(Name = "التذكرة")]
        public int TicketId { get; set; }

        public IEnumerable<SelectListItem>? Tickets { get; set; }

        [Display(Name = "نوع الخروج")]
        public int ExitTypeId { get; set; }

        public IEnumerable<SelectListItem>? ExitTypes { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; } = DateTimeOffset.Now;
    }
}