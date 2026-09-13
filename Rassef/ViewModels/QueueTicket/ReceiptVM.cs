namespace Rassef.ViewModels.QueueTicket
{
    public class ReceiptVM
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = "توريد";
        public string DepartmentName { get; set; } = string.Empty;
        public string DockName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string WaitingCount { get; set; } = "0";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        // كود التتبع المستخدم في رابط صفحة متابعة الدور (QR Code)
        public Guid TrackingCode { get; set; }

        // صورة QR Code جاهزة كـ Base64 Data URI لعرضها مباشرة في الـ <img>
        public string QrCodeImage { get; set; } = string.Empty;

        // مقاس ورق الطباعة (من إعدادات الطابعة) عشان نظبط الطباعة عليه بالظبط
        public int PrintWidthMm { get; set; } = 80;
        public int? PrintHeightMm { get; set; }
    }
}
