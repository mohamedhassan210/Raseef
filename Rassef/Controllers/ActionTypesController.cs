

namespace Rassef.Controllers
{
    public class ActionTypesController : Controller
    {
        private readonly IRepository<ActionTypes> _repository;

        public ActionTypesController(IRepository<ActionTypes> repository)
        {
            _repository = repository;
        }

        // GET: ActionTypes
        public async Task<IActionResult> Index()
        {
            var actionTypes = await _repository.GetAllAsync();

            var model = actionTypes.Select(x => new ActionTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                QueueActionsCount = x.QueueActions?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: ActionTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var actionType = await _repository.GetByIdAsync(id);

            if (actionType == null)
            {
                return NotFound();
            }

            var model = new ActionTypesDetailsVM
            {
                Id = actionType.Id,
                Name = actionType.Name,
                QueueActionsCount = actionType.QueueActions?.Count ?? 0
            };

            return View(model);
        }

        // GET: ActionTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateActionTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الإجراء مسجل بالفعل.");
                return View(model);
            }

            var actionType = new ActionTypes
            {
                Name = model.Name
            };

            await _repository.AddAsync(actionType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ActionTypes/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var actionType = await _repository.GetByIdAsync(id);

            if (actionType == null)
            {
                return NotFound();
            }

            var model = new UpdateActionTypesVM
            {
                Id = actionType.Id,
                Name = actionType.Name
            };

            return View(model);
        }

        // POST: ActionTypes/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateActionTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var actionType = await _repository.GetByIdAsync(model.Id);

            if (actionType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الإجراء مسجل بالفعل.");
                return View(model);
            }

            actionType.Name = model.Name;

            _repository.Update(actionType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ActionTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var actionType = await _repository.GetByIdAsync(id);

            if (actionType == null)
            {
                return NotFound();
            }

            var model = new ActionTypesDetailsVM
            {
                Id = actionType.Id,
                Name = actionType.Name,
                QueueActionsCount = actionType.QueueActions?.Count ?? 0
            };

            return View(model);
        }

        // POST: ActionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actionType = await _repository.GetByIdAsync(id);

            if (actionType == null)
            {
                return NotFound();
            }

            _repository.Remove(actionType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}