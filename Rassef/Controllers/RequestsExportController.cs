using System.Globalization;
using Rassef.Filters;
using Rassef.ViewModels.Administration;
using AppUser = Rassef.Models.Identity.User;

namespace Rassef.Controllers
{
    /// <summary>
    /// تصدير Excel لصفحتي "طلبات التحويل" و"طلبات التوريد" في لوحة التحكم.
    ///
    /// الصفحتين بتعمل الفلترة (تاريخ / قسم / بحث) عند الـ Browser، فالـ Browser هو اللي
    /// بيبعت الطلبات الظاهرة فعلاً على الشاشة + وصف الفلاتر، والسيرفر بيبني الملف من
    /// بيانات الطلبات الكاملة (موظف الحجز، موظف الخدمة، مدة الانتظار، مدة الخدمة...)
    /// مع Header وFooter. الصلاحية المطلوبة هي نفس صلاحية فتح الصفحة نفسها.
    /// </summary>
    public class RequestsExportController : Controller
    {
        private const string Placeholder = "—";
        private const int MaxRowsPerExport = 50000;

        private readonly ITransferRequestRepository _transferRepository;
        private readonly ISupplierRequestRepository _supplierRepository;
        private readonly ApplicationDbContext _db;

        // CHANGED — مفيش RequestsExcelReportService بالـ DI: بناء الملف بقى static
        // (ExcelReportBuilder)، فالتصدير مبيقعش لو تسجيل خدمة اتمسح من DependencyInjection.cs.
        public RequestsExportController(
            ITransferRequestRepository transferRepository,
            ISupplierRequestRepository supplierRepository,
            ApplicationDbContext db)
        {
            _transferRepository = transferRepository;
            _supplierRepository = supplierRepository;
            _db = db;
        }

        // ===================== طلبات التحويل =====================
        [HttpPost]
        [PermissionAuthorize("Administration", "TransferRequests")]
        public async Task<IActionResult> Transfer([FromBody] RequestsExportInput input)
        {
            if (input == null || input.Ids == null || input.Ids.Count == 0)
                return BadRequest(new { success = false, message = "لا توجد طلبات لتصديرها." });
            if (input.Ids.Count > MaxRowsPerExport)
                return BadRequest(new { success = false, message = "عدد الطلبات المطلوب تصديرها أكبر من الحد المسموح." });

            var all = await _transferRepository.GetAllWithDetailsAsync();
            var byId = all.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());

            var headers = new List<string>
            {
                "رقم الدور", "رقم الأفيز", "الحالة", "السائق", "الشاحنة", "الرصيف", "القسم",
                "موظف الحجز", "موظف الخدمة (المنادي)",
                "وقت الحجز", "وقت النداء", "وقت الخروج", "مدة الانتظار", "مدة الخدمة"
            };

            var rows = new List<string[]>();
            foreach (var id in input.Ids.Distinct())
            {
                if (!byId.TryGetValue(id, out var x)) continue;

                var ticket = LatestTicket(x.QueueTickets);
                var statusName = ticket?.TicketStatus?.Name ?? x.RequestStatus?.Name ?? "إنتظار";
                var statusClass = StatusClass(statusName);
                var times = BuildTimes(ticket, x.CreatedAT, statusClass);

                rows.Add(new[]
                {
                    ticket?.TicketNumber ?? Placeholder,
                    !string.IsNullOrWhiteSpace(x.AvizNumber) ? x.AvizNumber : Placeholder,
                    StatusText(statusClass),
                    OrPlaceholder(x.Driver?.FullName),
                    x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}".Trim() : Placeholder,
                    LatestDockName(ticket),
                    OrPlaceholder(x.Department?.Name),
                    BookingEmployeeName(x.CreatedBy, ticket?.CreatedBy),
                    UserDisplayName(x.Caller),
                    times.Booked, times.Called, times.Exit, times.Wait, times.Service
                });
            }

            var meta = await ExportMetaHelper.BuildAsync(HttpContext, _db, "طلبات تحويل", "طلبات التحويل", BuildFilterLines(input));
            var bytes = ExcelReportBuilder.BuildFromStrings(meta, headers, rows);
            return File(bytes, ExcelReportBuilder.ContentType, $"TransferRequests_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // ===================== طلبات التوريد =====================
        [HttpPost]
        [PermissionAuthorize("Administration", "SupplierRequests")]
        public async Task<IActionResult> Supplier([FromBody] RequestsExportInput input)
        {
            if (input == null || input.Ids == null || input.Ids.Count == 0)
                return BadRequest(new { success = false, message = "لا توجد طلبات لتصديرها." });
            if (input.Ids.Count > MaxRowsPerExport)
                return BadRequest(new { success = false, message = "عدد الطلبات المطلوب تصديرها أكبر من الحد المسموح." });

            var all = await _supplierRepository.GetAllWithDetailsAsync();
            var byId = all.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());

            var headers = new List<string>
            {
                "رقم الدور", "الحالة", "السائق", "الشاحنة", "الرصيف", "القسم", "الشركة", "رقم التصريح",
                "موظف الحجز", "موظف الخدمة (المنادي)",
                "وقت الحجز", "وقت النداء", "وقت الخروج", "مدة الانتظار", "مدة الخدمة"
            };

            var rows = new List<string[]>();
            foreach (var id in input.Ids.Distinct())
            {
                if (!byId.TryGetValue(id, out var x)) continue;

                var ticket = LatestTicket(x.QueueTickets);
                var statusName = ticket?.TicketStatus?.Name ?? "إنتظار";
                var statusClass = StatusClass(statusName);
                var times = BuildTimes(ticket, x.CreatedAT, statusClass);

                rows.Add(new[]
                {
                    ticket?.TicketNumber ?? Placeholder,
                    StatusText(statusClass),
                    OrPlaceholder(x.Driver?.FullName),
                    x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}".Trim() : Placeholder,
                    LatestDockName(ticket),
                    OrPlaceholder(x.Department?.Name),
                    OrPlaceholder(x.Supplier?.Name),
                    OrPlaceholder(x.PermitNumber),
                    BookingEmployeeName(x.CreatedBy, ticket?.CreatedBy),
                    UserDisplayName(x.Caller),
                    times.Booked, times.Called, times.Exit, times.Wait, times.Service
                });
            }

            var meta = await ExportMetaHelper.BuildAsync(HttpContext, _db, "طلبات توريد", "طلبات التوريد", BuildFilterLines(input));
            var bytes = ExcelReportBuilder.BuildFromStrings(meta, headers, rows);
            return File(bytes, ExcelReportBuilder.ContentType, $"SupplierRequests_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // ===================== سطور الفلاتر =====================

        /// <summary>سطور الفلاتر بتتكتب بس لو الفلتر مطبّق فعلاً.</summary>
        private static List<string> BuildFilterLines(RequestsExportInput input)
        {
            var lines = ExportFilterText.FromDateMode(input.Mode, input.From, input.To);

            if (!string.IsNullOrWhiteSpace(input.Department))
                lines.Add($"القسم: {input.Department.Trim()}");

            var search = ExportFilterText.Search(input.Search);
            if (search != null) lines.Add(search);

            return lines;
        }

        // ===================== أدوات مشتركة =====================

        private static QueueTicket? LatestTicket(IEnumerable<QueueTicket>? tickets) =>
            tickets?.OrderByDescending(q => q.CreatedAT).FirstOrDefault();

        private static string LatestDockName(QueueTicket? ticket)
        {
            var name = ticket?.DockAssignments?
                .OrderByDescending(da => da.AssignedAt)
                .Select(da => da.Dock?.DockName)
                .FirstOrDefault();
            return OrPlaceholder(name);
        }

        /// <summary>موظف الحجز = اللي أنشأ الطلب (نفس ترتيب الأولوية المستخدم في عمود "الموظف" بالصفحة).</summary>
        private static string BookingEmployeeName(AppUser? requestCreator, AppUser? ticketCreator)
        {
            foreach (var candidate in new[] { requestCreator, ticketCreator })
            {
                var name = UserDisplayName(candidate);
                if (name != Placeholder) return name;
            }
            return Placeholder;
        }

        private static string UserDisplayName(AppUser? user)
        {
            if (user == null) return Placeholder;
            if (!string.IsNullOrWhiteSpace(user.Name)) return user.Name;
            if (!string.IsNullOrWhiteSpace(user.UserName)) return user.UserName!;
            return Placeholder;
        }

        private static string OrPlaceholder(string? value) =>
            string.IsNullOrWhiteSpace(value) ? Placeholder : value!;

        private static string StatusClass(string? statusName)
        {
            var s = (statusName ?? "إنتظار").Replace("إ", "ا").Trim();
            if (s.Contains("مكتمل") || s.Contains("تم") || s.Contains("خروج") || s.Contains("منتهي")) return "completed";
            if (s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل")) return "in-progress";
            return "waiting";
        }

        private static string StatusText(string statusClass) => statusClass switch
        {
            "completed" => "تم",
            "in-progress" => "جاري",
            _ => "إنتظار"
        };

        private sealed record TimeColumns(string Booked, string Called, string Exit, string Wait, string Service);

        /// <summary>
        /// وقت الحجز = وقت إصدار الدور (QueueTime).
        /// وقت النداء = وقت دخول الدور "جاري" (EntryTime) — بيظهر بس لو الدور اتنادى عليه.
        /// مدة الانتظار = من الحجز لحد النداء.
        /// مدة الخدمة = من النداء لحد الخروج — بتظهر بس للأدوار المكتملة.
        /// أي وقت لسه ماحصلش بيظهر "—".
        /// </summary>
        private static TimeColumns BuildTimes(QueueTicket? ticket, DateTimeOffset fallbackBooked, string statusClass)
        {
            var booked = ticket != null && ticket.QueueTime != DateTimeOffset.MinValue ? ticket.QueueTime : fallbackBooked;

            var called = Placeholder;
            var exit = Placeholder;
            var wait = Placeholder;
            var service = Placeholder;

            if (ticket != null && statusClass != "waiting" && ticket.EntryTime != DateTimeOffset.MinValue)
            {
                called = FormatDateTime(ticket.EntryTime);
                var waitSpan = ticket.EntryTime - booked;
                if (waitSpan >= TimeSpan.Zero) wait = FormatDuration(waitSpan);

                if (statusClass == "completed"
                    && ticket.ExitTime != DateTimeOffset.MinValue
                    && ticket.ExitTime >= ticket.EntryTime)
                {
                    exit = FormatDateTime(ticket.ExitTime);
                    service = FormatDuration(ticket.ExitTime - ticket.EntryTime);
                }
            }

            return new TimeColumns(FormatDateTime(booked), called, exit, wait, service);
        }

        private static string FormatDuration(TimeSpan span) =>
            $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";

        private static string FormatDateTime(DateTimeOffset value) =>
            value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);

    }
}
