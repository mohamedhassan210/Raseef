

namespace Rassef.Controllers
{
    public class SupplyOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ExcelExportService _excelService;

        public SupplyOrderController(ApplicationDbContext context, ExcelExportService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromTime, DateTime? toTime, string? statusFilter, string? search)
        {
            ViewBag.FromTime = fromTime?.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.ToTime = toTime?.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.Search = search;
            var query = _context.SupplierRequests
                .Include(x => x.Driver)
                .Include(x => x.Truck)
                .Include(x => x.Department)
                .Include(x => x.RequestStatus)
                .Include(x => x.QueueTickets)
                    .ThenInclude(qt => qt.DockAssignments)
                        .ThenInclude(da => da.Dock)
                .AsQueryable();

            // الفلترة بالتاريخ والوقت اعتماداً على تاريخ إنشاء الطلب أو التذكرة
            if (fromTime.HasValue)
                query = query.Where(x => x.CreatedAT >= fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(x => x.CreatedAT <= toTime.Value);

            // الفلترة بحالة الطلب (انتظار / جاري / تم)
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "الكل")
                query = query.Where(x => x.RequestStatus.Name == statusFilter);

            // البحث برقم الدور أو اسم السائق أو رقم اللوحة
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Driver.FullName.Contains(search)
                                      || x.Truck.PlateNumber.Contains(search)
                                      || x.QueueTickets.Any(q => q.TicketNumber.Contains(search)));
            }

            var result = await query.Select(x => new SupplyOrderReportViewModel
            {
                RequestId = x.Id,
                // أخذ التذكرة المتصلة بالطلب
                QueueNumber = x.QueueTickets.Select(q => q.TicketNumber).FirstOrDefault() ?? "N/A",
                DriverName = x.Driver != null ? x.Driver.FullName : x.DriverPhone,
                TruckPlate = x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}" : "",
                DepartmentName = x.Department != null ? x.Department.Name : "",
                StatusName = x.RequestStatus != null ? x.RequestStatus.Name : "",
                // أخذ وقت الدخول من التذكرة أو وقت إنشاء الطلب
                EntryTime = x.QueueTickets.Select(q => q.EntryTime).FirstOrDefault() != default
                            ? x.QueueTickets.Select(q => q.EntryTime).FirstOrDefault()
                            : x.CreatedAT,
                // أخذ اسم الرصيف المعين للطلب
                DockName = x.QueueTickets
                            .SelectMany(q => q.DockAssignments)
                            .Select(da => da.Dock.DockName)
                            .FirstOrDefault() ?? "رصيف 5"
            }).ToListAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? fromTime, DateTime? toTime)
        {
            var query = _context.SupplierRequests
                .Include(x => x.Driver)
                .Include(x => x.Truck)
                .Include(x => x.Department)
                .Include(x => x.RequestStatus)
                .Include(x => x.QueueTickets)
                .AsQueryable();

            if (fromTime.HasValue) query = query.Where(x => x.CreatedAT >= fromTime.Value);
            if (toTime.HasValue) query = query.Where(x => x.CreatedAT <= toTime.Value);

            var reportData = await query.Select(x => new
            {
                رقم_الدور = x.QueueTickets.Select(q => q.TicketNumber).FirstOrDefault() ?? "N/A",
                اسم_السائق = x.Driver != null ? x.Driver.FullName : x.DriverPhone,
                رقم_السيارة = x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}" : "",
                القسم = x.Department != null ? x.Department.Name : "",
                الحالة = x.RequestStatus != null ? x.RequestStatus.Name : "",
                وقت_الدخول = x.CreatedAT.ToString("yyyy-MM-dd hh:mm tt")
            }).ToListAsync();

            var fileBytes = _excelService.ExportToExcel(reportData, "طلبات التوريد");

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SupplyOrders_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            );
        }
    }
}