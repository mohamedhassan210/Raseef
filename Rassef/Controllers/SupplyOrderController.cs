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

        /// <summary>
        /// تقرير أوامر وطلبات التوريد والبحث المتقدم بالتواريخ والأقسام والحالات
        /// Supply orders report page with date, department, and status filters
        /// </summary>
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

            if (fromTime.HasValue)
                query = query.Where(x => x.CreatedAT >= fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(x => x.CreatedAT <= toTime.Value);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "الكل")
                query = query.Where(x => x.RequestStatus != null && x.RequestStatus.Name == statusFilter);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => (x.Driver != null && x.Driver.FullName.Contains(search))
                                      || (x.Truck != null && x.Truck.PlateNumber.Contains(search))
                                      || x.QueueTickets.Any(q => q.TicketNumber != null && q.TicketNumber.Contains(search)));
            }

            var result = await query.Select(x => new SupplyOrderReportViewModel
            {
                RequestId = x.Id,
                QueueNumber = x.QueueTickets.Select(q => q.TicketNumber).FirstOrDefault() ?? "N/A",
                DriverName = x.Driver != null ? x.Driver.FullName : (x.DriverPhone ?? "غير محدد"),
                TruckPlate = x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}" : "غير محدد",
                DepartmentName = x.Department != null ? x.Department.Name : "غير محدد",
                StatusName = x.RequestStatus != null ? x.RequestStatus.Name : "غير محدد",
                EntryTime = x.QueueTickets.Select(q => q.EntryTime).FirstOrDefault() != default
                            ? x.QueueTickets.Select(q => q.EntryTime).FirstOrDefault()
                            : x.CreatedAT,
                DockName = x.QueueTickets
                            .SelectMany(q => q.DockAssignments)
                            .Select(da => da.Dock != null ? da.Dock.DockName : "A1")
                            .FirstOrDefault() ?? "A1"
            }).ToListAsync();

            return View(result);
        }

        /// <summary>
        /// تصدير تقرير أوامر التوريد إلى ملف Excel
        /// Exports supply orders report to Excel (.xlsx) file
        /// </summary>
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

        /// <summary>
        /// حذف أو إلغاء أمر توريد (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.SupplierRequests.FindAsync(id);
            if (request != null)
            {
                request.IsDeleted = true;
                request.MarkAsUpdated();
                _context.SupplierRequests.Update(request);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف أمر التوريد بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "أمر التوريد غير موجود.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}