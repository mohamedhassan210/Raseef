namespace Rassef.Controllers
{
    public class QueueTicketController : Controller
    {
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<TransferRequest> _transferRequestRepository;
        private readonly IRepository<SupplierRequest> _supplierRequestRepository;

        public QueueTicketController(
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
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var statuses = await _ticketStatusRepository.GetAllAsync();

            var listVM = tickets.Select(t => new QueueTicketListVM
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                DepartmentName = departments.FirstOrDefault(d => d.Id == t.DepartmentId)?.Name ?? "غير محدد",
                TicketStatusName = statuses.FirstOrDefault(s => s.Id == t.TicketStatusId)?.Name ?? "غير محدد",
                QueueTime = t.QueueTime,
                EntryTime = t.EntryTime,
                ExitTime = t.ExitTime
            }).ToList();

            return View(listVM);
        }

        [HttpGet]
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
        public async Task<IActionResult> Create()
        {
            var model = new CreateQueueTicketVM();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                ModelState.AddModelError(string.Empty, "القسم غير موجود.");
                await PopulateDropdowns(model);
                return View(model);
            }

            var today = DateTime.Today;

            var tickets = await _ticketRepository.GetAllAsync();

            var lastTicket = tickets
                .Where(x => x.DepartmentId == model.DepartmentId &&
                            x.CreatedAT.Date == today)
                .OrderByDescending(x => x.CreatedAT)
                .FirstOrDefault();

            int counter = 1;

            if (lastTicket != null)
            {
                counter = int.Parse(lastTicket.TicketNumber.Substring(1)) + 1;
            }

            var ticketNumber = $"{department.Name}{counter}";

            var ticket = new QueueTicket
            {
                TicketNumber = ticketNumber,
                DepartmentId = model.DepartmentId,
                TicketStatusId = model.TicketStatusId,
                TransferRequestId = model.TransferRequestId,
                SupplierRequestId = model.SupplierRequestId,
                QueueTime = DateTimeOffset.Now,
                EntryTime = DateTimeOffset.MinValue,
                ExitTime = DateTimeOffset.MinValue
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء التذكرة رقم {ticketNumber}";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
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