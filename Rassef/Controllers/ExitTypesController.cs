

namespace Rassef.Controllers
{
    public class ExitTypesController : Controller
    {
        private readonly IRepository<ExitTypes> _repository;

        public ExitTypesController(IRepository<ExitTypes> repository)
        {
            _repository = repository;
        }

        // GET: ExitTypes
        public async Task<IActionResult> Index()
        {
            var exitTypes = await _repository.GetAllAsync();

            var model = exitTypes.Select(x => new ExitTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                CheckOutsCount = x.CheckOuts?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: ExitTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var exitType = await _repository.GetByIdAsync(id);

            if (exitType == null)
            {
                return NotFound();
            }

            var model = new ExitTypesDetailsVM
            {
                Id = exitType.Id,
                Name = exitType.Name,
                CheckOutsCount = exitType.CheckOuts?.Count ?? 0
            };

            return View(model);
        }

        // GET: ExitTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ExitTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExitTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع الخروج مسجل بالفعل.");
                return View(model);
            }

            var exitType = new ExitTypes
            {
                Name = model.Name
            };

            await _repository.AddAsync(exitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ExitTypes/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var exitType = await _repository.GetByIdAsync(id);

            if (exitType == null)
            {
                return NotFound();
            }

            var model = new UpdateExitTypesVM
            {
                Id = exitType.Id,
                Name = exitType.Name
            };

            return View(model);
        }

        // POST: ExitTypes/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateExitTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var exitType = await _repository.GetByIdAsync(model.Id);

            if (exitType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع الخروج مسجل بالفعل.");
                return View(model);
            }

            exitType.Name = model.Name;

            _repository.Update(exitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ExitTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var exitType = await _repository.GetByIdAsync(id);

            if (exitType == null)
            {
                return NotFound();
            }

            var model = new ExitTypesDetailsVM
            {
                Id = exitType.Id,
                Name = exitType.Name,
                CheckOutsCount = exitType.CheckOuts?.Count ?? 0
            };

            return View(model);
        }

        // POST: ExitTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exitType = await _repository.GetByIdAsync(id);

            if (exitType == null)
            {
                return NotFound();
            }

            _repository.Remove(exitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}