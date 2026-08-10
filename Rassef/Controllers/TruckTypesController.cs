

namespace Rassef.Controllers
{
    public class TruckTypesController : Controller
    {
        private readonly IRepository<TruckTypes> _repository;

        public TruckTypesController(IRepository<TruckTypes> repository)
        {
            _repository = repository;
        }

        // GET: TruckTypes
        public async Task<IActionResult> Index()
        {
            var truckTypes = await _repository.GetAllAsync();

            var model = truckTypes.Select(x => new TruckTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                TrucksCount = x.Trucks?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: TruckTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var truckType = await _repository.GetByIdAsync(id);

            if (truckType == null)
            {
                return NotFound();
            }

            var model = new TruckTypesDetailsVM
            {
                Id = truckType.Id,
                Name = truckType.Name,
                TrucksCount = truckType.Trucks?.Count ?? 0
            };

            return View(model);
        }

        // GET: TruckTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TruckTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTruckTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع الشاحنة مسجل بالفعل.");
                return View(model);
            }

            var truckType = new TruckTypes
            {
                Name = model.Name
            };

            await _repository.AddAsync(truckType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TruckTypes/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var truckType = await _repository.GetByIdAsync(id);

            if (truckType == null)
            {
                return NotFound();
            }

            var model = new UpdateTruckTypesVM
            {
                Id = truckType.Id,
                Name = truckType.Name
            };

            return View(model);
        }

        // POST: TruckTypes/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTruckTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var truckType = await _repository.GetByIdAsync(model.Id);

            if (truckType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع الشاحنة مسجل بالفعل.");
                return View(model);
            }

            truckType.Name = model.Name;

            _repository.Update(truckType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TruckTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var truckType = await _repository.GetByIdAsync(id);

            if (truckType == null)
            {
                return NotFound();
            }

            var model = new TruckTypesDetailsVM
            {
                Id = truckType.Id,
                Name = truckType.Name,
                TrucksCount = truckType.Trucks?.Count ?? 0
            };

            return View(model);
        }

        // POST: TruckTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var truckType = await _repository.GetByIdAsync(id);

            if (truckType == null)
            {
                return NotFound();
            }

            _repository.Remove(truckType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}