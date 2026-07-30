namespace Rassef.ViewModels.CheckOut
{
    public class CheckOutDetailsVM
    {
        [Display(Name = "المعرف")]
        public int Id { get; set; }


        [Display(Name = "رقم التذكرة")]
        public string TicketNumber { get; set; } = string.Empty;
        [Display(Name = "نوع الخروج")]
        public string ExitTypeName { get; set; } = string.Empty;

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }

        [Display(Name = "تم الإنشاء بواسطة")]
        public string CreatedBy { get; set; } = string.Empty;
    }
}