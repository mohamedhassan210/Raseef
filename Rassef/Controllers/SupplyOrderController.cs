namespace Rassef.Controllers
{
    public class SupplyOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        // CHANGED — شلنا ExcelExportService من الـ constructor: لو تسجيله اتمسح من
        // DependencyInjection.cs الـ controller كله كان بيقع (مش بس التصدير). التصدير بقى
        // بيستخدم ExcelReportBuilder (static) فمفيش حاجة تتسجّل.
        public SupplyOrderController(ApplicationDbContext context)
        {
            _context = context;
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
            var result = await BuildReportAsync(fromTime, toTime, statusFilter, search);

            return View(result);
        }

        /// <summary>
        /// تصدير تقرير أوامر التوريد إلى ملف Excel (.xlsx)
        /// CHANGED — بيطبّق نفس فلاتر الصفحة بالظبط (من/إلى + الحالة + البحث، كان بياخد
        /// من/إلى بس)، ونفس بيانات الجدول، ومعاه Header (العنوان + المخزن + الفلاتر)
        /// وFooter (محرّر التقرير + تاريخ الإصدار).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? fromTime, DateTime? toTime, string? statusFilter, string? search)
        {
            var report = await BuildReportAsync(fromTime, toTime, statusFilter, search);

            var headers = new List<string>
            {
                "رقم الدور", "اسم السائق", "رقم السيارة", "الرصيف", "القسم", "الحالة", "وقت الدخول"
            };

            var rows = report.Select(x => new[]
            {
                x.QueueNumber,
                x.DriverName,
                x.TruckPlate,
                x.DockName,
                x.DepartmentName,
                x.StatusName,
                x.EntryTime.ToString("yyyy-MM-dd hh:mm tt", System.Globalization.CultureInfo.InvariantCulture)
            }).ToList();

            var filterLines = new List<string>();
            var period = ExportFilterText.Period(fromTime, toTime);
            if (period != null) filterLines.Add(period);
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "الكل") filterLines.Add($"الحالة: {statusFilter}");
            var searchLine = ExportFilterText.Search(search);
            if (searchLine != null) filterLines.Add(searchLine);

            var meta = await ExportMetaHelper.BuildAsync(HttpContext, _context, "أوامر التوريد", "أوامر التوريد", filterLines);
            var bytes = ExcelReportBuilder.BuildFromStrings(meta, headers, rows);

            return File(bytes, ExcelReportBuilder.ContentType, $"SupplyOrders_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        /// <summary>نفس الاستعلام والفلاتر للصفحة وللتصدير — عشان الملف يطابق اللي المستخدم شايفه.</summary>
        private async Task<List<SupplyOrderReportViewModel>> BuildReportAsync(
            DateTime? fromTime, DateTime? toTime, string? statusFilter, string? search)
        {
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

            return await query.Select(x => new SupplyOrderReportViewModel
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