

namespace Rassef.Controllers
{
    public class DockStatusesController : Controller
    {
        private readonly IRepository<DockStatuses> _repository;

        public DockStatusesController(IRepository<DockStatuses> repository)
        {
            _repository = repository;
        }

        // GET: DockStatuses
        public async Task<IActionResult> Index()
        {
            var dockStatuses = await _repository.GetAllAsync();

            var model = dockStatuses.Select(x => new DockStatusesListVM
            {
                Id = x.Id,
                Name = x.Name,
                DocksCount = x.Docks?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: DockStatuses/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dockStatus = await _repository.GetByIdAsync(id);

            if (dockStatus == null)
            {
                return NotFound();
            }

            var model = new DockStatusesDetailsVM
            {
                Id = dockStatus.Id,
                Name = dockStatus.Name,
                DocksCount = dockStatus.Docks?.Count ?? 0
            };

            return View(model);
        }

        // GET: DockStatuses/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DockStatuses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDockStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة الرصيف مسجل بالفعل.");
                return View(model);
            }

            var dockStatus = new DockStatuses
            {
                Name = model.Name
            };

            await _repository.AddAsync(dockStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DockStatuses/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var dockStatus = await _repository.GetByIdAsync(id);

            if (dockStatus == null)
            {
                return NotFound();
            }

            var model = new UpdateDockStatusesVM
            {
                Id = dockStatus.Id,
                Name = dockStatus.Name
            };

            return View(model);
        }

        // POST: DockStatuses/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDockStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dockStatus = await _repository.GetByIdAsync(model.Id);

            if (dockStatus == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة الرصيف مسجل بالفعل.");
                return View(model);
            }

            dockStatus.Name = model.Name;

            _repository.Update(dockStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DockStatuses/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dockStatus = await _repository.GetByIdAsync(id);

            if (dockStatus == null)
            {
                return NotFound();
            }

            var model = new DockStatusesDetailsVM
            {
                Id = dockStatus.Id,
                Name = dockStatus.Name,
                DocksCount = dockStatus.Docks?.Count ?? 0
            };

            return View(model);
        }

        // POST: DockStatuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dockStatus = await _repository.GetByIdAsync(id);

            if (dockStatus == null)
            {
                return NotFound();
            }

            _repository.Remove(dockStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}