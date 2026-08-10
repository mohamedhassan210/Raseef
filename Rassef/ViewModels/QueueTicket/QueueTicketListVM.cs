namespace Rassef.ViewModels.QueueTicket
{
    public class QueueTicketListVM
    {
        public int Id { get; set; }

        [Display(Name = "رقم التذكرة")]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "اسم السائق")]
        public string DriverName { get; set; } = string.Empty;

        [Display(Name = "رقم السيارة")]
        public string TruckNumber { get; set; } = string.Empty;

        [Display(Name = "القسم")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "الرصيف")]
        public string DockName { get; set; } = string.Empty;

        [Display(Name = "حالة التذكرة")]
        public string TicketStatusName { get; set; } = string.Empty;

        [Display(Name = "وقت الانتظار")]
        public DateTimeOffset QueueTime { get; set; }

        [Display(Name = "وقت الدخول")]
        public DateTimeOffset EntryTime { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}