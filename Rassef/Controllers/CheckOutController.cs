namespace Rassef.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly ICheckOutRepository _repository;
        private readonly IRepository<QueueTicket> _ticketRepo;
        private readonly IRepository<ExitTypes> _exitTypeRepo;
        private readonly IRepository<TicketStatuses> _ticketStatusRepo;

        public CheckOutController(
            ICheckOutRepository repository,
            IRepository<QueueTicket> ticketRepo,
            IRepository<ExitTypes> exitTypeRepo,
            IRepository<TicketStatuses> ticketStatusRepo)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _ticketRepo = ticketRepo ?? throw new ArgumentNullException(nameof(ticketRepo));
            _exitTypeRepo = exitTypeRepo ?? throw new ArgumentNullException(nameof(exitTypeRepo));
            _ticketStatusRepo = ticketStatusRepo ?? throw new ArgumentNullException(nameof(ticketStatusRepo));
        }

        [HttpGet]
        // Display all items - CQ-8: استخدام GetAllWithDetailsAsync لتفادي NullReferenceException
        public async Task<IActionResult> Index()
        {
            var checkOuts = await _repository.GetAllWithDetailsAsync();

            var checkOutViewModels = checkOuts.Select(x => new CheckOutListVM
            {
                Id = x.Id,
                TicketNumber = x.QueueTicket?.TicketNumber ?? "غير محدد",
                ExitTypeName = x.ExitType?.Name ?? "غير محدد",
                ExitTime = x.ExitTime
            });

            return View(checkOutViewModels);
        }

        [HttpGet]
        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var checkOut = await _repository.GetByIdAsync(id);

            if (checkOut == null)
            {
                ModelState.AddModelError("تسجيل الخروج", "تسجيل الخروج هذا غير موجود");
                return View();
            }

            var model = new CheckOutDetailsVM
            {
                Id = id,
                TicketNumber = checkOut.QueueTicket?.TicketNumber ?? "غير محدد",
                ExitTypeName = checkOut.ExitType?.Name ?? "غير محدد",
                ExitTime = checkOut.ExitTime,
                CreatedBy = checkOut.CreatedBy?.Name ?? "النظام"
            };

            return View(model);
        }

        [HttpGet]
        // Display create page
        public async Task<IActionResult> Create()
        {
            var vm = await PopulateCreateDropdownsAsync(new CreateCheckOutVM());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item - BL-5: تحديث حالة التذكرة إلى "تم" عند الخروج
        public async Task<IActionResult> Create(CreateCheckOutVM create)
        {
            if (!ModelState.IsValid)
            {
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var checkOut = new CheckOut
            {
                TicketId = create.TicketId,
                ExitTypeId = create.ExitTypeId,
                ExitTime = create.ExitTime
            };

            await _repository.AddAsync(checkOut);
            await _repository.SaveChangesAsync();

            // BL-5: تحديث حالة التذكرة إلى "تم" تلقائياً
            var ticket = await _ticketRepo.GetByIdAsync(create.TicketId);
            if (ticket != null)
            {
                var doneStatus = await _ticketStatusRepo.FindAsync(s =>
                    s.Name.Contains("تم") || s.Name.Contains("مكتمل"));
                if (doneStatus != null)
                {
                    ticket.TicketStatusId = doneStatus.Id;
                    ticket.ExitTime = create.ExitTime;
                    _ticketRepo.Update(ticket);
                    await _ticketRepo.SaveChangesAsync();
                }
            }

            TempData["Success"] = "تم تسجيل الخروج بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display update page
        public async Task<IActionResult> Update(int id)
        {
            var checkOut = await _repository.GetByIdAsync(id);

            if (checkOut == null)
            {
                ModelState.AddModelError("تسجيل الخروج", "تسجيل الخروج هذا غير موجود");
                return View();
            }

            var vm = new UpdateCheckOutVM
            {
                Id = checkOut.Id,
                TicketId = checkOut.TicketId,
                ExitTypeId = checkOut.ExitTypeId,
                ExitTime = checkOut.ExitTime
            };

            vm = await PopulateUpdateDropdownsAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Update item
        public async Task<IActionResult> Update(UpdateCheckOutVM updateVm)
        {
            if (!ModelState.IsValid)
            {
                updateVm = await PopulateUpdateDropdownsAsync(updateVm);
                return View(updateVm);
            }

            var checkOut = await _repository.GetByIdAsync(updateVm.Id);

            if (checkOut == null)
            {
                ModelState.AddModelError("تسجيل الخروج", "تسجيل الخروج هذا غير موجود");
                return View();
            }

            checkOut.TicketId = updateVm.TicketId;
            checkOut.ExitTypeId = updateVm.ExitTypeId;
            checkOut.ExitTime = updateVm.ExitTime;

            _repository.Update(checkOut);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var checkOut = await _repository.GetByIdAsync(id);

            if (checkOut == null)
            {
                ModelState.AddModelError("تسجيل الخروج", "تسجيل الخروج هذا غير موجود");
                return View();
            }

            var vm = new CheckOutDetailsVM
            {
                Id = checkOut.Id,
                TicketNumber = checkOut.QueueTicket?.TicketNumber ?? "غير محدد"
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkOut = await _repository.GetByIdAsync(id);

            if (checkOut == null)
            {
                ModelState.AddModelError("تسجيل الخروج", "تسجيل الخروج هذا غير موجود");
                return View();
            }

            _repository.Remove(checkOut);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private async Task<(IEnumerable<SelectListItem> Tickets, IEnumerable<SelectListItem> ExitTypes)> GetDropdownDataAsync()
        {
            var tickets = await _ticketRepo.GetAllAsync();
            var exitTypes = await _exitTypeRepo.GetAllAsync();

            return (
                tickets.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TicketNumber.ToString() }),
                exitTypes.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            );
        }

        private async Task<CreateCheckOutVM> PopulateCreateDropdownsAsync(CreateCheckOutVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Tickets = data.Tickets;
            vm.ExitTypes = data.ExitTypes;
            return vm;
        }

        private async Task<UpdateCheckOutVM> PopulateUpdateDropdownsAsync(UpdateCheckOutVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Tickets = data.Tickets;
            vm.ExitTypes = data.ExitTypes;
            return vm;
        }

        #endregion
    }
}
