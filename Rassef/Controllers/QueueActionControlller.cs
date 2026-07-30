
namespace Rassef.Controllers
{
    public class QueueActionController : Controller
    {
        private readonly IRepository<QueueAction> _queueActionRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<ActionTypes> _actionTypeRepository;


        public QueueActionController(
            IRepository<QueueAction> queueActionRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<ActionTypes> actionTypeRepository
            )
        {
            _queueActionRepository = queueActionRepository;
            _ticketRepository = ticketRepository;
            _actionTypeRepository = actionTypeRepository;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var actions = await _queueActionRepository.GetAllAsync();
            var actionTypes = await _actionTypeRepository.GetAllAsync();

            var listVM = actions.Select(a => new QueueActionListVM
            {
                Id = a.Id,
                TicketId = a.TicketId,
                ActionTypeName = actionTypes.FirstOrDefault(t => t.Id == a.ActionTypeId)?.Name ?? "غير محدد",
                ActionTime = a.ActionTime
            }).ToList();

            return View(listVM);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var action = await _queueActionRepository.GetByIdAsync(id);

            if (action == null)
            {
                ModelState.AddModelError(string.Empty, "الإجراء المطلوب غير موجود.");
                return View(new QueueActionDetailsVM());
            }

            var actionType = await _actionTypeRepository.GetByIdAsync(action.ActionTypeId);

            var detailsVM = new QueueActionDetailsVM
            {
                Id = action.Id,
                TicketId = action.TicketId,
                ActionTypeName = actionType?.Name ?? "غير محدد",
                ActionTime = action.ActionTime,
                CreatedAT = action.CreatedAT
            };

            return View(detailsVM);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateQueueActionVM();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQueueActionVM model)
        {
            if (model is null)
            {
                ModelState.AddModelError(string.Empty, " لا يوجد بيانات .");
                await PopulateDropdowns(model);
                return View(model);
            }

            var queueAction = new QueueAction
            {
                TicketId = model.TicketId,
                ActionTypeId = model.ActionTypeId,
                ActionTime = model.ActionTime
            };

            await _queueActionRepository.AddAsync(queueAction);
            await _queueActionRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة الإجراء بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var action = await _queueActionRepository.GetByIdAsync(id);

            if (action == null)
            {
                ModelState.AddModelError(string.Empty, "الإجراء المطلوب غير موجود للتعديل.");
                var emptyModel = new UpdateQueueActionVM();
                await PopulateDropdowns(emptyModel);
                return View(emptyModel);
            }

            var model = new UpdateQueueActionVM
            {
                Id = action.Id,
                TicketId = action.TicketId,
                ActionTypeId = action.ActionTypeId,
                ActionTime = action.ActionTime
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateQueueActionVM model)
        {

            if (model is null) {
                ModelState.AddModelError(string.Empty, " لا يوجد بيانات .");
                await PopulateDropdowns(model);
                return View(model);
            }

            var action = await _queueActionRepository.GetByIdAsync(model.Id);

            if (action == null)
            {
                ModelState.AddModelError(string.Empty, "تعذر التعديل، الإجراء غير موجود بالسجلات.");
                await PopulateDropdowns(model);
                return View(model);
            }

            action.TicketId = model.TicketId;
            action.ActionTypeId = model.ActionTypeId;
            action.ActionTime = model.ActionTime;

            _queueActionRepository.Update(action);
            await _queueActionRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل الإجراء بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var action = await _queueActionRepository.GetByIdAsync(id);

            if (action == null)
            {
                TempData["ErrorMessage"] = "تعذر الحذف، الإجراء غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            _queueActionRepository.Remove(action);
            await _queueActionRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الإجراء بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(CreateQueueActionVM model)
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var actionTypes = await _actionTypeRepository.GetAllAsync();

            model.Tickets = tickets.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.TicketNumber
            });

            model.ActionTypes = actionTypes.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            });
        }

        private async Task PopulateDropdowns(UpdateQueueActionVM model)
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var actionTypes = await _actionTypeRepository.GetAllAsync();

            model.Tickets = tickets.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.TicketNumber
            });

            model.ActionTypes = actionTypes.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            });
        }
    }
}