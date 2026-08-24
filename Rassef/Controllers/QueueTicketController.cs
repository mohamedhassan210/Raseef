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
        private readonly IUserRepository _userRepository;
        private readonly ITicketEngineService _ticketEngineService;

        public QueueTicketController(
            IRepository<QueueSettings> queueSettingsRepository,
            IRepository<Shift> shiftRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<Department> departmentRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IRepository<TransferRequest> transferRequestRepository,
            IRepository<SupplierRequest> supplierRequestRepository,
            IUserRepository userRepository,
            ITicketEngineService ticketEngineService)
        {
            _ticketRepository = ticketRepository;
            _departmentRepository = departmentRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _transferRequestRepository = transferRequestRepository;
            _supplierRequestRepository = supplierRequestRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _shiftRepository = shiftRepository;
            _userRepository = userRepository;
            _ticketEngineService = ticketEngineService;
        }
        /// <summary>
        /// صفحة قائمة الأدوار الحية (Gate Live Queue)
        /// Live Queue dashboard for gate operations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await LiveQueue();
        }

        /// <summary>
        /// عرض شاشة الأدوار الحية والإحصائيات اللحظية
        /// Displays live queue status board with current active and waiting trucks
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LiveQueue()
        {
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.Shift)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Supplier)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Driver)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Truck)
                .Include(t => t.TransferRequest!)
                    .ThenInclude(tr => tr!.Driver)
                .Include(t => t.TransferRequest!)
                    .ThenInclude(tr => tr!.Truck)
                .Include(t => t.DockAssignments!)
                    .ThenInclude(da => da.Dock)
            );

            var ticketViewModels = ticketsList.Select(t =>
            {
                var dockAssignment = t.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                bool isSupplier = t.SupplierRequestId != null || t.SupplierRequest != null;
                string reqType = isSupplier ? "توريد" : "تحويل";
                string company = isSupplier ? (t.SupplierRequest?.Supplier?.Name ?? "غير محدد") : "تحويل داخلي";

                return new QueueTicketListVM
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber ?? "A1",
                    TicketStatusName = t.TicketStatus != null ? t.TicketStatus.Name : "إنتظار",
                    DriverName = t.SupplierRequest?.Driver?.FullName ?? t.TransferRequest?.Driver?.FullName ?? "غير محدد",
                    TruckNumber = t.SupplierRequest?.Truck != null ? $"{t.SupplierRequest.Truck.PlateLetter} {t.SupplierRequest.Truck.PlateNumber}" : (t.TransferRequest?.Truck != null ? $"{t.TransferRequest.Truck.PlateLetter} {t.TransferRequest.Truck.PlateNumber}" : "غير محدد"),
                    DepartmentName = t.Department?.Name ?? "غير محدد",
                    DockName = dockAssignment?.Dock?.DockName ?? "A1",
                    RequestType = reqType,
                    CompanyName = company,
                    QueueTime = t.QueueTime,
                    EntryTime = t.EntryTime != DateTimeOffset.MinValue ? t.EntryTime : t.CreatedAT,
                    ExitTime = t.ExitTime
                };
            }).ToList();

            int waitingCount = 0, inProgressCount = 0, completedCount = 0;
            foreach (var t in ticketViewModels)
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                if (s.Contains("انتظار") || s.Contains("طابور") || s.Contains("معلق"))
                    waitingCount++;
                else if (s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل"))
                    inProgressCount++;
                else if (s.Contains("تم") || s.Contains("مكتمل") || s.Contains("منتهي") || s.Contains("خروج"))
                    completedCount++;
            }

            int GetStatusPriority(string statusName)
            {
                var s = statusName.Replace("إ", "ا").Trim();
                if (s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل")) return 1;
                if (s.Contains("انتظار") || s.Contains("طابور") || s.Contains("معلق")) return 2;
                if (s.Contains("تم") || s.Contains("مكتمل") || s.Contains("منتهي") || s.Contains("خروج")) return 3;
                return 4;
            }

            var orderedViewModels = ticketViewModels
                .OrderBy(t => GetStatusPriority(t.TicketStatusName))
                .ThenByDescending(t => t.Id)
                .ToList();

            var dockLetters = new[] { "A", "B", "C", "D", "E", "F" };
            var dockNames = new[] { "رصيف 1", "رصيف 2", "رصيف 3", "رصيف 4", "رصيف 5", "رصيف 6" };

            var inProgressList = ticketViewModels.Where(t =>
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                return s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل");
            }).OrderByDescending(t => t.EntryTime).ToList();

            var waitingList = ticketViewModels.Where(t =>
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                bool isDone = s.Contains("تم") || s.Contains("مكتمل") || s.Contains("خروج") || s.Contains("منتهي");
                bool isActive = s.Contains("جاري") || s.Contains("تنفيذ");
                return !isDone && !isActive;
            }).OrderBy(t => t.QueueTime).ToList();

            var dockCards = new List<LiveDockCardVM>();
            for (int i = 0; i < 6; i++)
            {
                string letter = dockLetters[i];
                string name = dockNames[i];

                var currentTicket = inProgressList.FirstOrDefault(t => t.DockName.Contains((i + 1).ToString()) || t.DockName.Contains(letter))
                    ?? (inProgressList.Count > i ? inProgressList[i] : null);

                var nextTicket = waitingList.FirstOrDefault(t => t.DockName.Contains((i + 1).ToString()) || t.DockName.Contains(letter))
                    ?? (waitingList.Count > i ? waitingList[i] : null);

                dockCards.Add(new LiveDockCardVM
                {
                    DockLetter = letter,
                    DockName = name,
                    CurrentTicketNumber = currentTicket?.TicketNumber ?? (inProgressList.FirstOrDefault()?.TicketNumber ?? "A265"),
                    NextTicketNumber = nextTicket?.TicketNumber ?? (waitingList.FirstOrDefault()?.TicketNumber ?? "A266"),
                    Status = currentTicket != null ? "جاري" : "إنتظار"
                });
            }

            var viewModel = new QueueTicketIndexVM
            {
                Tickets = orderedViewModels,
                DockCards = dockCards,
                WaitingCount = waitingCount,
                InProgressCount = inProgressCount,
                CompletedCount = completedCount
            };

            return View("LiveQueue", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveDisplayData()
        {
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.DockAssignments!)
                    .ThenInclude(da => da.Dock)
            );

            var ticketViewModels = ticketsList.Select(t =>
            {
                var dockAssignment = t.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                return new
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber ?? "A1",
                    TicketStatusName = t.TicketStatus != null ? t.TicketStatus.Name : "إنتظار",
                    DockName = dockAssignment?.Dock?.DockName ?? "A1",
                    QueueTime = t.QueueTime,
                    EntryTime = t.EntryTime
                };
            }).ToList();

            var dockLetters = new[] { "A", "B", "C", "D", "E", "F" };
            var dockNames = new[] { "رصيف 1", "رصيف 2", "رصيف 3", "رصيف 4", "رصيف 5", "رصيف 6" };

            var inProgressList = ticketViewModels.Where(t =>
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                return s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل");
            }).OrderByDescending(t => t.EntryTime).ToList();

            var waitingList = ticketViewModels.Where(t =>
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                bool isDone = s.Contains("تم") || s.Contains("مكتمل") || s.Contains("خروج") || s.Contains("منتهي");
                bool isActive = s.Contains("جاري") || s.Contains("تنفيذ");
                return !isDone && !isActive;
            }).OrderBy(t => t.QueueTime).ToList();

            var dockCards = new List<object>();
            for (int i = 0; i < 6; i++)
            {
                string letter = dockLetters[i];
                string name = dockNames[i];

                var currentTicket = inProgressList.FirstOrDefault(t => t.DockName.Contains((i + 1).ToString()) || t.DockName.Contains(letter))
                    ?? (inProgressList.Count > i ? inProgressList[i] : null);

                var nextTicket = waitingList.FirstOrDefault(t => t.DockName.Contains((i + 1).ToString()) || t.DockName.Contains(letter))
                    ?? (waitingList.Count > i ? waitingList[i] : null);

                dockCards.Add(new
                {
                    dockLetter = letter,
                    dockName = name,
                    currentTicketNumber = currentTicket?.TicketNumber ?? (inProgressList.FirstOrDefault()?.TicketNumber ?? "A265"),
                    nextTicketNumber = nextTicket?.TicketNumber ?? (waitingList.FirstOrDefault()?.TicketNumber ?? "A266"),
                    status = currentTicket != null ? "جاري" : "إنتظار"
                });
            }

            return Json(new { success = true, dockCards });
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

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var uId))
            {
                currentUserId = uId;
            }

            var ticketResult = await _ticketEngineService.IssueGeneralTicketAsync(
                departmentId: model.DepartmentId,
                supplierRequestId: model.SupplierRequestId,
                transferRequestId: model.TransferRequestId,
                ticketStatusId: model.TicketStatusId,
                userId: currentUserId
            );

            TempData["SuccessMessage"] = $"تم إنشاء التذكرة رقم {ticketResult.TicketNumber}";

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

        // استدعاء الدور القادم
        [HttpPost]
        public async Task<IActionResult> CallNext(int? departmentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.CallNextTicketAsync(departmentId, currentUserId);
            return Json(result);
        }

        // تحديث حالة التذكرة
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateTicketStatusDTO dto)
        {
            if (dto == null || dto.TicketId <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            {
                return Json(new { success = false, message = "بيانات غير صالحة." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.UpdateTicketStatusAsync(dto.TicketId, dto.Status, currentUserId);
            return Json(result);
        }

        // =========================================================================
        // 1. شاشة استدعاء الأدوار المتتالية (Next Next Next Station)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> CallStation(int? departmentId)
        {
            var model = await BuildCallStationVM(departmentId);
            ViewBag.Departments = await _departmentRepository.GetAllAsync();
            return View("CallStation", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStationState(int? departmentId)
        {
            var model = await BuildCallStationVM(departmentId);
            return Json(new
            {
                success = true,
                currentTicket = model.CurrentTicket,
                nextTicket = model.NextUpcomingTicket,
                waitingCount = model.WaitingCount,
                inProgressCount = model.InProgressCount,
                completedCount = model.CompletedTodayCount,
                waitingQueue = model.WaitingQueue.Take(5).Select(t => new
                {
                    ticketNumber = t.TicketNumber,
                    truckNumber = t.TruckNumber,
                    driverName = t.DriverName,
                    companyName = t.CompanyName,
                    requestType = t.RequestType,
                    dockName = t.DockName,
                    departmentName = t.DepartmentName
                })
            });
        }

        private async Task<CallStationVM> BuildCallStationVM(int? departmentId)
        {
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Supplier)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Driver)
                .Include(t => t.SupplierRequest!)
                    .ThenInclude(sr => sr!.Truck)
                .Include(t => t.TransferRequest!)
                    .ThenInclude(tr => tr!.Driver)
                .Include(t => t.TransferRequest!)
                    .ThenInclude(tr => tr!.Truck)
                .Include(t => t.DockAssignments!)
                    .ThenInclude(da => da.Dock)
            );

            var mappedTickets = ticketsList
                .Where(t => !departmentId.HasValue || departmentId.Value <= 0 || t.DepartmentId == departmentId.Value)
                .Select(t =>
                {
                    var dockAssignment = t.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                    bool isSupplier = t.SupplierRequestId != null || t.SupplierRequest != null;
                    string reqType = isSupplier ? "توريد" : "تحويل";
                    string company = isSupplier ? (t.SupplierRequest?.Supplier?.Name ?? "غير محدد") : "تحويل داخلي";

                    return new QueueTicketListVM
                    {
                        Id = t.Id,
                        TicketNumber = t.TicketNumber ?? "A1",
                        TicketStatusName = t.TicketStatus != null ? t.TicketStatus.Name : "إنتظار",
                        DriverName = t.SupplierRequest?.Driver?.FullName ?? t.TransferRequest?.Driver?.FullName ?? "غير محدد",
                        TruckNumber = t.SupplierRequest?.Truck != null ? $"{t.SupplierRequest.Truck.PlateLetter} {t.SupplierRequest.Truck.PlateNumber}" : (t.TransferRequest?.Truck != null ? $"{t.TransferRequest.Truck.PlateLetter} {t.TransferRequest.Truck.PlateNumber}" : "غير محدد"),
                        DepartmentName = t.Department?.Name ?? "غير محدد",
                        DockName = dockAssignment?.Dock?.DockName ?? "A1",
                        RequestType = reqType,
                        CompanyName = company,
                        QueueTime = t.QueueTime,
                        EntryTime = t.EntryTime != DateTimeOffset.MinValue ? t.EntryTime : t.CreatedAT,
                        ExitTime = t.ExitTime
                    };
                }).ToList();

            var inProgressTickets = mappedTickets
                .Where(t =>
                {
                    var n = (t.TicketStatusName ?? "").Replace("إ", "ا").Trim();
                    return n.Contains("جاري") || n.Contains("تنفيذ") || n.Contains("تشغيل");
                })
                .OrderByDescending(t => t.EntryTime)
                .ToList();

            var waitingTickets = mappedTickets
                .Where(t =>
                {
                    var n = (t.TicketStatusName ?? "").Replace("إ", "ا").Trim();
                    bool isDone = n.Contains("تم") || n.Contains("مكتمل") || n.Contains("خروج");
                    bool isActive = n.Contains("جاري") || n.Contains("تنفيذ");
                    return !isDone && !isActive;
                })
                .OrderBy(t => t.QueueTime)
                .ThenBy(t => t.Id)
                .ToList();

            int completedCount = mappedTickets.Count(t =>
            {
                var n = (t.TicketStatusName ?? "").Replace("إ", "ا").Trim();
                return n.Contains("تم") || n.Contains("مكتمل") || n.Contains("خروج");
            });

            var recentlyCompletedTickets = mappedTickets
                .Where(t =>
                {
                    var n = (t.TicketStatusName ?? "").Replace("إ", "ا").Trim();
                    return n.Contains("تم") || n.Contains("مكتمل") || n.Contains("خروج");
                })
                .OrderByDescending(t => t.ExitTime)
                .ThenByDescending(t => t.Id)
                .ToList();

            return new CallStationVM
            {
                CurrentTicket = inProgressTickets.FirstOrDefault(),
                NextUpcomingTicket = waitingTickets.FirstOrDefault(),
                WaitingQueue = waitingTickets,
                WaitingCount = waitingTickets.Count,
                InProgressCount = inProgressTickets.Count,
                CompletedTodayCount = completedCount,
                SelectedDepartmentId = departmentId
            };
        }

        // =========================================================================
        // 2. شاشة بوابة الخروج وإنهاء التذاكر بالكود (Exit Gate & Checkout)
        // =========================================================================
        [HttpGet]
        public IActionResult ExitGate()
        {
            return View("ExitGate");
        }

        /// <summary>
        /// البحث عن تذكرة برقم التذكرة أو كود التتبع أو رقم اللوحة لمعاينة بيانات الخروج
        /// Searches for active ticket by number/code/plate for checkout inspection
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchTicketForCheckout(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "يرجى إدخال رقم أو كود التذكرة." });
            }

            var cleanQuery = query.Trim();
            var tickets = await _ticketRepository.GetAllAsync(q => q
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Supplier)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Driver)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Truck)
                .Include(t => t.TransferRequest!).ThenInclude(tr => tr!.Driver)
                .Include(t => t.TransferRequest!).ThenInclude(tr => tr!.Truck)
                .Include(t => t.DockAssignments!).ThenInclude(da => da.Dock)
            );

            var ticket = tickets.FirstOrDefault(t =>
                string.Equals(t.TicketNumber, cleanQuery, StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(cleanQuery, out var id) && t.Id == id) ||
                (t.SupplierRequest?.Truck != null && (t.SupplierRequest.Truck.PlateNumber.Contains(cleanQuery) || $"{t.SupplierRequest.Truck.PlateLetter} {t.SupplierRequest.Truck.PlateNumber}".Contains(cleanQuery))) ||
                (t.TransferRequest?.Truck != null && (t.TransferRequest.Truck.PlateNumber.Contains(cleanQuery) || $"{t.TransferRequest.Truck.PlateLetter} {t.TransferRequest.Truck.PlateNumber}".Contains(cleanQuery)))
            );

            if (ticket == null)
            {
                return Json(new { success = false, message = $"لم يتم العثور على أي تذكرة برقم أو كود: {cleanQuery}" });
            }

            var dockAssignment = ticket.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
            bool isSupplier = ticket.SupplierRequestId != null || ticket.SupplierRequest != null;
            string reqType = isSupplier ? "توريد" : "تحويل";
            string company = isSupplier ? (ticket.SupplierRequest?.Supplier?.Name ?? "غير محدد") : "تحويل داخلي";
            string truckPlate = ticket.SupplierRequest?.Truck != null
                ? $"{ticket.SupplierRequest.Truck.PlateLetter} {ticket.SupplierRequest.Truck.PlateNumber}"
                : (ticket.TransferRequest?.Truck != null ? $"{ticket.TransferRequest.Truck.PlateLetter} {ticket.TransferRequest.Truck.PlateNumber}" : "غير محدد");
            string driverName = ticket.SupplierRequest?.Driver?.FullName
                ?? ticket.TransferRequest?.Driver?.FullName ?? "غير محدد";

            var entryTime = ticket.EntryTime != DateTimeOffset.MinValue ? ticket.EntryTime : ticket.CreatedAT;
            var duration = DateTimeOffset.Now - entryTime;
            string durationFormatted = $"{(int)duration.TotalHours} ساعة و {duration.Minutes} دقيقة";

            return Json(new
            {
                success = true,
                ticketId = ticket.Id,
                ticketNumber = ticket.TicketNumber,
                truckPlate = truckPlate,
                driverName = driverName,
                companyOrType = company,
                requestType = reqType,
                departmentName = ticket.Department?.Name ?? "غير محدد",
                dockName = dockAssignment?.Dock?.DockName ?? "A1",
                status = ticket.TicketStatus?.Name ?? "إنتظار",
                entryTimeFormatted = entryTime.ToString("yyyy/MM/dd hh:mm tt"),
                durationFormatted = durationFormatted,
                isAlreadyCompleted = (ticket.TicketStatus?.Name ?? "").Contains("مكتمل") || (ticket.TicketStatus?.Name ?? "").Contains("تم")
            });
        }

        /// <summary>
        /// تسجيل خروج الشاحنة وتحويل حالة الدور إلى مكتمل وتحديث وقت الخروج
        /// Checks out the truck, sets ticket status to Completed, and marks ExitTime
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckoutTicket([FromBody] CheckoutTicketDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TicketCode))
            {
                return Json(new { success = false, message = "يرجى إدخال كود أو رقم التذكرة." });
            }

            var cleanQuery = dto.TicketCode.Trim();
            var tickets = await _ticketRepository.GetAllAsync(q => q
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Supplier)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Driver)
                .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Truck)
                .Include(t => t.TransferRequest!).ThenInclude(tr => tr!.Driver)
                .Include(t => t.TransferRequest!).ThenInclude(tr => tr!.Truck)
                .Include(t => t.DockAssignments!).ThenInclude(da => da.Dock)
            );

            var ticket = tickets.FirstOrDefault(t =>
                string.Equals(t.TicketNumber, cleanQuery, StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(cleanQuery, out var id) && t.Id == id) ||
                (t.SupplierRequest?.Truck != null && t.SupplierRequest.Truck.PlateNumber == cleanQuery) ||
                (t.TransferRequest?.Truck != null && t.TransferRequest.Truck.PlateNumber == cleanQuery)
            );

            if (ticket == null)
            {
                return Json(new { success = false, message = $"لا توجد تذكرة برقم: {cleanQuery}" });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var updateResult = await _ticketEngineService.UpdateTicketStatusAsync(ticket.Id, "مكتمل", currentUserId);
            if (!updateResult.Success)
            {
                return Json(new { success = false, message = updateResult.Message });
            }

            var dockAssignment = ticket.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
            bool isSupplier = ticket.SupplierRequestId != null || ticket.SupplierRequest != null;
            string company = isSupplier ? (ticket.SupplierRequest?.Supplier?.Name ?? "غير محدد") : "تحويل داخلي";
            string truckPlate = ticket.SupplierRequest?.Truck != null
                ? $"{ticket.SupplierRequest.Truck.PlateLetter} {ticket.SupplierRequest.Truck.PlateNumber}"
                : (ticket.TransferRequest?.Truck != null ? $"{ticket.TransferRequest.Truck.PlateLetter} {ticket.TransferRequest.Truck.PlateNumber}" : "غير محدد");
            string driverName = ticket.SupplierRequest?.Driver?.FullName
                ?? ticket.TransferRequest?.Driver?.FullName ?? "غير محدد";

            var entryTime = ticket.EntryTime != DateTimeOffset.MinValue ? ticket.EntryTime : ticket.CreatedAT;
            var exitTime = DateTimeOffset.Now;
            var duration = exitTime - entryTime;
            string durationFormatted = $"{(int)duration.TotalHours} ساعة و {duration.Minutes} دقيقة";

            return Json(new CheckoutResultDto
            {
                Success = true,
                Message = $"✅ تم تسجيل خروج الشاحنة وإنهاء التذكرة رقم {ticket.TicketNumber} بنجاح.",
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                TruckPlate = truckPlate,
                DriverName = driverName,
                CompanyOrType = company,
                DepartmentName = ticket.Department?.Name ?? "غير محدد",
                DockName = dockAssignment?.Dock?.DockName ?? "A1",
                EntryTime = entryTime,
                ExitTime = exitTime,
                DurationFormatted = durationFormatted
            });
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
