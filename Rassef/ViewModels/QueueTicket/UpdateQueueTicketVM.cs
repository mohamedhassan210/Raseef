

namespace Rassef.ViewModels.QueueTicket
{
    public class UpdateQueueTicketVM
    {
        public int Id { get; set; }

        [Display(Name = "رقم التذكرة")]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "طلب التحويل (اختياري)")]
        public int? TransferRequestId { get; set; }
        public IEnumerable<SelectListItem>? TransferRequests { get; set; }

        [Display(Name = "طلب المورد (اختياري)")]
        public int? SupplierRequestId { get; set; }
        public IEnumerable<SelectListItem>? SupplierRequests { get; set; }

        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }

        [Display(Name = "حالة التذكرة")]
        public int TicketStatusId { get; set; }
        public IEnumerable<SelectListItem>? TicketStatuses { get; set; }

        [Display(Name = "وقت الانتظار")]
        public DateTimeOffset QueueTime { get; set; }

        [Display(Name = "وقت الدخول")]
        public DateTimeOffset EntryTime { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}