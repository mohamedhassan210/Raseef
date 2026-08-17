namespace Rassef.Controllers
{
    public class DockAssignmentController : Controller
    {
        private readonly IDockAssignmentRepository _assignmentRepository;
        private readonly IRepository<Dock> _dockRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<User> _userRepository;

        public DockAssignmentController(
            IDockAssignmentRepository assignmentRepository,
            IRepository<Dock> dockRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<User> userRepository)
        {
            _assignmentRepository = assignmentRepository;
            _dockRepository = dockRepository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
        }

        // Get All - CQ-7: استخدام GetAllWithDetailsAsync لجلب Dock و QueueTicket
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var assignmentsRepo = await _assignmentRepository.GetAllWithDetailsAsync();

            var assignments = assignmentsRepo.Select(x => new DockAssignmentListVM
            {
                Id = x.Id,
                DockName = x.Dock?.DockName ?? "غير محدد",
                TicketNumber = x.QueueTicket?.TicketNumber ?? "غير محدد",
                AssignedAt = x.AssignedAt,
                FinishedAt = x.FinishedAt
            }).ToList();

            return View(assignments);
        }

        // Get Details
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("التعيين", "رقم التعيين مفقود.");
                return View(new DockAssignmentDetailsVM());
            }

            var assignment = await _assignmentRepository.GetByIdAsync(id.Value);

            if (assignment == null)
            {
                ModelState.AddModelError("التعيين", "هذا التعيين غير موجود.");
                return View(new DockAssignmentDetailsVM());
            }

            var assignmentDetails = new DockAssignmentDetailsVM
            {
                Id = assignment.Id,
                DockName = assignment.Dock?.DockName ?? "غير محدد",
                TicketNumber = assignment.QueueTicket?.TicketNumber ?? "غير محدد",
                AssignedAt = assignment.AssignedAt,
                FinishedAt = assignment.FinishedAt,
                CreatedByName = assignment.CreatedBy?.Name ?? "النظام"
            };

            return View(assignmentDetails);
        }

        // Get Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateDockAssignmentVM
            {
                AssignedAt = DateTimeOffset.Now,
                FinishedAt = DateTimeOffset.Now.AddHours(1)
            };

            vm = await PopulateCreateDropdownsAsync(vm);
            return View(vm);
        }

        // Post Create - BL-6: منع تكرار تعيين نفس التذكرة لأكثر من رصيف في نفس الوقت
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDockAssignmentVM create)
        {
            if (!ModelState.IsValid)
            {
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            // BL-6: التحقق من عدم وجود تعيين نشط لنفس التذكرة
            bool alreadyAssigned = await _assignmentRepository.ExistsAsync(
                a => a.TicketId == create.TicketId && a.FinishedAt > DateTimeOffset.Now);

            if (alreadyAssigned)
            {
                ModelState.AddModelError("", "هذه التذكرة معينة لرصيف آخر بالفعل. أنهِ التعيين الحالي أولاً.");
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError("", "يجب تسجيل الدخول أولاً.");
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var currentUser = await _userRepository.GetByIdAsync(int.Parse(userId));

            if (currentUser is null)
            {
                ModelState.AddModelError("", "لم يتم العثور على المستخدم.");
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var assignment = new DockAssignment
            {
                DockId = create.DockId,
                TicketId = create.TicketId,
                AssignedAt = create.AssignedAt,
                FinishedAt = create.FinishedAt,
                CreatedBy = currentUser
            };

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة تعيين الرصيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // Get Update
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("التعيين", "رقم التعيين مفقود.");
                return View(await PopulateUpdateDropdownsAsync(new UpdateDockAssignmentVM()));
            }

            var assignment = await _assignmentRepository.GetByIdAsync(id.Value);

            if (assignment == null)
            {
                ModelState.AddModelError("التعيين", "هذا التعيين غير موجود.");
                return View(await PopulateUpdateDropdownsAsync(new UpdateDockAssignmentVM()));
            }

            var vm = new UpdateDockAssignmentVM
            {
                Id = assignment.Id,
                DockId = assignment.DockId,
                TicketId = assignment.TicketId,
                AssignedAt = assignment.AssignedAt,
                FinishedAt = assignment.FinishedAt
            };

            vm = await PopulateUpdateDropdownsAsync(vm);
            return View(vm);
        }

        // Post Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDockAssignmentVM update)
        {
            if (!ModelState.IsValid)
            {
                update = await PopulateUpdateDropdownsAsync(update);
                return View(update);
            }

            var assignment = await _assignmentRepository.GetByIdAsync(update.Id);

            if (assignment == null)
            {
                ModelState.AddModelError("التعيين", "هذا التعيين غير موجود.");
                update = await PopulateUpdateDropdownsAsync(update);
                return View(update);
            }

            assignment.DockId = update.DockId;
            assignment.TicketId = update.TicketId;
            assignment.AssignedAt = update.AssignedAt;
            assignment.FinishedAt = update.FinishedAt;

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات التعيين بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // Get Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("التعيين", "رقم التعيين مفقود.");
                return View(new DockAssignmentDetailsVM());
            }

            var assignment = await _assignmentRepository.GetByIdAsync(id.Value);

            if (assignment == null)
            {
                ModelState.AddModelError("التعيين", "هذا التعيين غير موجود.");
                return View(new DockAssignmentDetailsVM());
            }

            var vm = new DockAssignmentDetailsVM
            {
                Id = assignment.Id,
                DockName = assignment.Dock?.DockName ?? "غير محدد",
                TicketNumber = assignment.QueueTicket?.TicketNumber ?? "غير محدد",
                AssignedAt = assignment.AssignedAt,
                FinishedAt = assignment.FinishedAt,
                CreatedByName = assignment.CreatedBy?.Name ?? "غير محدد"
            };

            return View(vm);
        }

        // Post Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);

            if (assignment == null)
            {
                ModelState.AddModelError("التعيين", "هذا التعيين غير موجود.");
                return View(new DockAssignmentDetailsVM());
            }

            _assignmentRepository.Remove(assignment);
            await _assignmentRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف التعيين بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private async Task<CreateDockAssignmentVM> PopulateCreateDropdownsAsync(CreateDockAssignmentVM vm)
        {
            var docks = await _dockRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();

            vm.Docks = docks.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DockName });
            vm.Tickets = tickets.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TicketNumber });

            return vm;
        }

        private async Task<UpdateDockAssignmentVM> PopulateUpdateDropdownsAsync(UpdateDockAssignmentVM vm)
        {
            var docks = await _dockRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();

            vm.Docks = docks.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DockName });
            vm.Tickets = tickets.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TicketNumber });

            return vm;
        }

        #endregion
    }
}
