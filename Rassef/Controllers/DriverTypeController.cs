

namespace Rassef.Controllers
{
    public class DriverTypeController : Controller
    {
        private readonly IRepository<DriverTypes> _repository;

        public DriverTypeController(IRepository<DriverTypes> repository)
        {
            _repository = repository;
        }

        // GET: DriverType
        public async Task<IActionResult> Index()
        {
            var driverTypes = await _repository.GetAllAsync();

            var model = driverTypes.Select(x => new DriverTypeListVM
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DriversCount = x.Drivers?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: DriverType/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var driverType = await _repository.GetByIdAsync(id);

            if (driverType == null)
            {
                return NotFound();
            }

            var model = new DriverTypeDetailsVM
            {
                Id = driverType.Id,
                Code = driverType.Code,
                Name = driverType.Name,
                DriversCount = driverType.Drivers?.Count ?? 0
            };

            return View(model);
        }

        // GET: DriverType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DriverType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع السائق مسجل بالفعل.");
                return View(model);
            }

            var driverType = new DriverTypes
            {
                Code = model.Code,
                Name = model.Name
            };

            await _repository.AddAsync(driverType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DriverType/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var driverType = await _repository.GetByIdAsync(id);

            if (driverType == null)
            {
                return NotFound();
            }

            var model = new UpdateDriverTypeVM
            {
                Id = driverType.Id,
                Code = driverType.Code,
                Name = driverType.Name
            };

            return View(model);
        }

        // POST: DriverType/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDriverTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var driverType = await _repository.GetByIdAsync(model.Id);

            if (driverType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع السائق مسجل بالفعل.");
                return View(model);
            }

            driverType.Code = model.Code;
            driverType.Name = model.Name;

            _repository.Update(driverType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DriverType/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var driverType = await _repository.GetByIdAsync(id);

            if (driverType == null)
            {
                return NotFound();
            }

            var model = new DriverTypeDetailsVM
            {
                Id = driverType.Id,
                Code = driverType.Code,
                Name = driverType.Name,
                DriversCount = driverType.Drivers?.Count ?? 0
            };

            return View(model);
        }

        // POST: DriverType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driverType = await _repository.GetByIdAsync(id);

            if (driverType == null)
            {
                return NotFound();
            }

            _repository.Remove(driverType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}