namespace Rassef.Controllers
{
    public class QueueTicketController : Controller
    {
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<TransferRequest> _transferRequestRepository;
        private readonly IRepository<SupplierRequest> _supplierRequestRepository;
        private readonly IRepository<QueueSettings> _queueSettingsRepository;
        private readonly IRepository<Shift> _shiftRepository;

        public QueueTicketController(
            IRepository<QueueSettings> queueSettingsRepository,
            IRepository<Shift> shiftRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<Department> departmentRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IRepository<TransferRequest> transferRequestRepository,
            IRepository<SupplierRequest> supplierRequestRepository)
        {
            _ticketRepository = ticketRepository;
            _departmentRepository = departmentRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _transferRequestRepository = transferRequestRepository;
            _supplierRequestRepository = supplierRequestRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _shiftRepository = shiftRepository;
        }
        //  الادوار الحالية 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tickets = await _ticketRepository.GetAllAsync(query =>
                query
                    .Include(t => t.TransferRequest)
                        .ThenInclude(r => r.Driver)

                    .Include(t => t.TransferRequest)
                        .ThenInclude(r => r.Truck)

                    .Include(t => t.SupplierRequest)
                        .ThenInclude(r => r.Driver)

                    .Include(t => t.SupplierRequest)
                        .ThenInclude(r => r.Truck)

                    .Include(t => t.DockAssignments)
                        .ThenInclude(a => a.Dock)
            );

            var departments = await _departmentRepository.GetAllAsync();
            var statuses = await _ticketStatusRepository.GetAllAsync();

            var ticketsList = tickets.Select(t =>
            {
                string driverName = "غير محدد";
                string truckNumber = "غير محدد";

                if (t.TransferRequest != null)
                {
                    driverName = t.TransferRequest.Driver?.FullName ?? "غير محدد";
                    truckNumber = t.TransferRequest.Truck?.PlateNumber ?? "غير محدد";
                }
                else if (t.SupplierRequest != null)
                {
                    driverName = t.SupplierRequest.Driver?.FullName ?? "غير محدد";
                    truckNumber = t.SupplierRequest.Truck?.PlateNumber ?? "غير محدد";
                }

                var dockAssignment = t.DockAssignments?
                    .OrderByDescending(x => x.AssignedAt)
                    .FirstOrDefault();

                return new QueueTicketListVM
                {
                    Id = t.Id,

                    TicketNumber = t.TicketNumber,

                    DriverName = driverName,

                    TruckNumber = truckNumber,

                    DepartmentName = departments
                        .FirstOrDefault(d => d.Id == t.DepartmentId)?.Name
                        ?? "غير محدد",

                    DockName = dockAssignment?.Dock?.DockName
                        ?? "غير محدد",

                    TicketStatusName = statuses
                        .FirstOrDefault(s => s.Id == t.TicketStatusId)?.Name
                        ?? "غير محدد",

                    QueueTime = t.QueueTime,

                    EntryTime = t.EntryTime,

                    ExitTime = t.ExitTime
                };
            }).ToList();

            var viewModel = new QueueTicketIndexVM
            {
                Tickets = ticketsList,

                CompletedCount = ticketsList.Count(x =>
                    x.TicketStatusName == "تم"),

                InProgressCount = ticketsList.Count(x =>
                    x.TicketStatusName == "جاري"),

                WaitingCount = ticketsList.Count(x =>
                    x.TicketStatusName == "انتظار")
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> viewRole()
        {
            // 1. جلب كل التذاكر مع الـ Includes المطلوبة عشان السائق والشاحنة والقسم والرصيف يظهروا
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.Shift)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr.Driver)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr.Truck)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr.Driver)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr.Truck)
            // لو عندك علاقة للأرصفة (DockAssignments) تقدر تضيفها هنا
            );

            // 2. تحويل الـ Data لـ ViewModel (لو عندك Mapper أو هتعملها يدوي)
            var ticketViewModels = ticketsList.Select(t => new QueueTicketListVM
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                TicketStatusName = t.TicketStatus != null ? t.TicketStatus.Name : "انتظار",
                DriverName = t.SupplierRequest != null ? t.SupplierRequest.Driver.FullName :
                             (t.TransferRequest != null ? t.TransferRequest.Driver.FullName : "غير متوفر"),
                TruckNumber = t.SupplierRequest != null ? t.SupplierRequest.Truck.PlateNumber :
                              (t.TransferRequest != null ? t.TransferRequest.Truck.PlateNumber : "غير متوفر"),
                DepartmentName = t.Department != null ? t.Department.Name : "غير متوفر",
                DockName = "رصيف 5", // تقدر تربطها بجدول DockAssignments لو مربوطة فعلياً
                EntryTime = t.EntryTime
            }).ToList();

            // 3. حساب الإحصائيات (الانتظار، الجاري، تم)
            int waitingCount = ticketViewModels.Count(x => x.TicketStatusName == "انتظار" || x.TicketStatusName == "في الطابور");
            int inProgressCount = ticketViewModels.Count(x => x.TicketStatusName == "جاري" || x.TicketStatusName == "قيد التنفيذ");
            int completedCount = ticketViewModels.Count(x => x.TicketStatusName == "تم" || x.TicketStatusName == "مكتملة");

            // 4. تجميع الموديل النهائي للـ View
            var viewModel = new QueueTicketIndexVM
            {
                Tickets = ticketViewModels,
                WaitingCount = waitingCount,
                InProgressCount = inProgressCount,
                CompletedCount = completedCount
            };

            return View(viewModel);
        }

        [HttpGet]
        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                ModelState.AddModelError(string.Empty, "التذكرة المطلوبة غير موجودة.");
                return View(new QueueTicketDetailsVM());
            }

            var department = await _departmentRepository.GetByIdAsync(ticket.DepartmentId);
            var status = await _ticketStatusRepository.GetByIdAsync(ticket.TicketStatusId);

            var detailsVM = new QueueTicketDetailsVM
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                DepartmentName = department?.Name ?? "غير محدد",
                TicketStatusName = status?.Name ?? "غير محدد",
                QueueTime = ticket.QueueTime,
                EntryTime = ticket.EntryTime,
                ExitTime = ticket.ExitTime,
                TransferRequestInfo = ticket.TransferRequestId.HasValue ? $"طلب تحويل #{ticket.TransferRequestId}" : "لا يوجد",
                SupplierRequestInfo = ticket.SupplierRequestId.HasValue ? $"طلب مورد #{ticket.SupplierRequestId}" : "لا يوجد",
                CreatedByUserName = ticket.CreatedBy?.UserName ?? "غير محدد",
                DockAssignmentsCount = ticket.DockAssignments?.Count ?? 0,
                QueueActionsCount = ticket.QueueActions?.Count ?? 0,
                HasCheckedOut = ticket.CheckOut != null
            };

            return View(detailsVM);
        }

        [HttpGet]
        // Display create page
        public async Task<IActionResult> Create()
        {
            var model = new CreateQueueTicketVM();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item
        public async Task<IActionResult> Create(CreateQueueTicketVM model)
        {
            if (model.DepartmentId <= 0)
                ModelState.AddModelError(nameof(model.DepartmentId), "يرجى اختيار القسم.");

            if (model.TicketStatusId <= 0)
                ModelState.AddModelError(nameof(model.TicketStatusId), "يرجى اختيار حالة التذكرة.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var department = await _departmentRepository.GetByIdAsync(model.DepartmentId);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم غير موجود.");
                await PopulateDropdowns(model);
                return View(model);
            }

            var settings = await _queueSettingsRepository.FindAsync(x => true);

            if (settings == null)
            {
                ModelState.AddModelError("", "لم يتم إعداد نظام العد.");
                await PopulateDropdowns(model);
                return View(model);
            }

            Shift? shift = null;

            if (settings.ResetType == ResetType.ByShift)
            {
                if (settings.ShiftId == null)
                {
                    ModelState.AddModelError("", "لم يتم اختيار الشيفت.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                shift = await _shiftRepository.GetByIdAsync(settings.ShiftId.Value);

                if (shift == null)
                {
                    ModelState.AddModelError("", "الشيفت غير موجود.");
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }

            var tickets = await _ticketRepository.GetAllAsync();

            DateTimeOffset resetDate = DateTimeOffset.MinValue;

            switch (settings.ResetType)
            {
                case ResetType.Daily:

                    resetDate = DateTimeOffset.Now.Date;

                    break;

                case ResetType.ByShift:

                    var shiftStart = DateTime.Today.Add(shift!.StartTime);
                    var shiftEnd = shiftStart.Add(shift.Duration);

                    resetDate = shiftStart;

                    if (shift.LastResetAt.HasValue && shift.LastResetAt > resetDate)
                        resetDate = shift.LastResetAt.Value;

                    break;

                case ResetType.Manual:

                    resetDate = settings.LastGlobalResetAt ?? DateTimeOffset.MinValue;

                    break;
            }

            if (department.LastResetAt.HasValue && department.LastResetAt > resetDate)
                resetDate = department.LastResetAt.Value;

            if (settings.LastGlobalResetAt.HasValue && settings.LastGlobalResetAt > resetDate)
                resetDate = settings.LastGlobalResetAt.Value;

            var lastTicket = tickets
                .Where(x => x.DepartmentId == model.DepartmentId &&
                            x.CreatedAT >= resetDate)
                .OrderByDescending(x => x.CreatedAT)
                .FirstOrDefault();

            int counter = 1;

            if (lastTicket != null)
            {
                var digits = new string(lastTicket.TicketNumber
                    .Where(char.IsDigit)
                    .ToArray());

                if (!string.IsNullOrWhiteSpace(digits))
                    counter = int.Parse(digits) + 1;
            }

            var ticketNumber = $"{department.Prefix}{counter}";

            var ticket = new QueueTicket
            {
                TicketNumber = ticketNumber,
                DepartmentId = model.DepartmentId,
                TicketStatusId = model.TicketStatusId,
                TransferRequestId = model.TransferRequestId,
                SupplierRequestId = model.SupplierRequestId,
                QueueTime = DateTimeOffset.Now,
                EntryTime = DateTimeOffset.MinValue,
                ExitTime = DateTimeOffset.MinValue,
                ShiftId = settings.ResetType == ResetType.ByShift ? settings.ShiftId : null
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء التذكرة رقم {ticketNumber}";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Action Edit
        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                ModelState.AddModelError(string.Empty, "التذكرة المطلوبة غير موجودة للتعديل.");
                var emptyModel = new UpdateQueueTicketVM();
                await PopulateDropdowns(emptyModel);
                return View(emptyModel);
            }

            var model = new UpdateQueueTicketVM
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                DepartmentId = ticket.DepartmentId,
                TicketStatusId = ticket.TicketStatusId,
                TransferRequestId = ticket.TransferRequestId,
                SupplierRequestId = ticket.SupplierRequestId,
                QueueTime = ticket.QueueTime,
                EntryTime = ticket.EntryTime,
                ExitTime = ticket.ExitTime
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Action Edit
        public async Task<IActionResult> Edit(UpdateQueueTicketVM model)
        {
            if (model.Id <= 0)
            {
                ModelState.AddModelError(nameof(model.Id), "معرف التذكرة مطلوب.");
            }

            if (string.IsNullOrWhiteSpace(model.TicketNumber))
            {
                ModelState.AddModelError(nameof(model.TicketNumber), "رقم التذكرة مطلوب.");
            }

            if (model.DepartmentId <= 0)
            {
                ModelState.AddModelError(nameof(model.DepartmentId), "يرجى اختيار القسم.");
            }

            if (model.TicketStatusId <= 0)
            {
                ModelState.AddModelError(nameof(model.TicketStatusId), "يرجى اختيار حالة التذكرة.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var ticket = await _ticketRepository.GetByIdAsync(model.Id);

            if (ticket == null)
            {
                ModelState.AddModelError(string.Empty, "تعذر التعديل، التذكرة غير موجودة بالسجلات.");
                await PopulateDropdowns(model);
                return View(model);
            }

            ticket.TicketNumber = model.TicketNumber;
            ticket.DepartmentId = model.DepartmentId;
            ticket.TicketStatusId = model.TicketStatusId;
            ticket.TransferRequestId = model.TransferRequestId;
            ticket.SupplierRequestId = model.SupplierRequestId;
            ticket.QueueTime = model.QueueTime;
            ticket.EntryTime = model.EntryTime;
            ticket.ExitTime = model.ExitTime;

            _ticketRepository.Update(ticket);
            await _ticketRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل بيانات التذكرة بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                TempData["ErrorMessage"] = "تعذر الحذف، التذكرة غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

            _ticketRepository.Remove(ticket);
            await _ticketRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف التذكرة بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(CreateQueueTicketVM model)
        {
            var departments = await _departmentRepository.GetAllAsync();
            var statuses = await _ticketStatusRepository.GetAllAsync();
            var transfers = await _transferRequestRepository.GetAllAsync();
            var suppliers = await _supplierRequestRepository.GetAllAsync();

            model.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            model.TicketStatuses = statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.TransferRequests = transfers.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"طلب تحويل #{t.Id}" });
            model.SupplierRequests = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"طلب مورد #{s.Id}" });
        }

        private async Task PopulateDropdowns(UpdateQueueTicketVM model)
        {
            var departments = await _departmentRepository.GetAllAsync();
            var statuses = await _ticketStatusRepository.GetAllAsync();
            var transfers = await _transferRequestRepository.GetAllAsync();
            var suppliers = await _supplierRequestRepository.GetAllAsync();

            model.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            model.TicketStatuses = statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.TransferRequests = transfers.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"طلب تحويل #{t.Id}" });
            model.SupplierRequests = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"طلب مورد #{s.Id}" });
        }
    }
}
