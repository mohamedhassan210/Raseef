
namespace Rassef.ViewModels.QueueTicket
{
    public class QueueTicketDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "رقم التذكرة")]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "طلب التحويل")]
        public string? TransferRequestInfo { get; set; }

        [Display(Name = "طلب المورد")]
        public string? SupplierRequestInfo { get; set; }

        [Display(Name = "القسم")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "حالة التذكرة")]
        public string TicketStatusName { get; set; } = string.Empty;

        [Display(Name = "وقت الانتظار")]
        public DateTimeOffset QueueTime { get; set; }

        [Display(Name = "وقت الدخول")]
        public DateTimeOffset EntryTime { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }

        [Display(Name = "تم الإنشائ بواسطة")]
        public string CreatedByUserName { get; set; } = string.Empty;

        [Display(Name = "عدد التخصيصات (Dock Assignments)")]
        public int DockAssignmentsCount { get; set; }

        [Display(Name = "عدد الإجراءات (Queue Actions)")]
        public int QueueActionsCount { get; set; }

        [Display(Name = "حالة الخروج (CheckOut)")]
        public bool HasCheckedOut { get; set; }
    }
}