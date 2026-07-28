namespace Rassef.ViewModels.CheckOut
{
    public class CheckOutListVM
    {
        [Display(Name = "المعرف")]
        public Guid Id { get; set; }

        [Display(Name = "التذكرة")]
        public Guid TicketId { get; set; }

        [Display(Name = "نوع الخروج")]
        public string ExitTypeName { get; set; } = string.Empty;

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}