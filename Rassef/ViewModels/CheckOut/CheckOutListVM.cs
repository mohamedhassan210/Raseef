namespace Rassef.ViewModels.CheckOut
{
    public class CheckOutListVM
    {
        [Display(Name = "المعرف")]
        public int Id { get; set; }

        [Display(Name = "رقم التذكرة")]
        public string TicketNumber { get; set; } = string.Empty;
        [Display(Name = "نوع الخروج")]
        public string ExitTypeName { get; set; } = string.Empty;

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}