
namespace Rassef.Controllers
{
    public class CommodityTypesController : Controller
    {
        private readonly IRepository<CommodityTypes> _repository;

        public CommodityTypesController(IRepository<CommodityTypes> repository)
        {
            _repository = repository;
        }

        // GET: CommodityTypes
        public async Task<IActionResult> Index()
        {
            var commodityTypes = await _repository.GetAllAsync();

            var model = commodityTypes.Select(x => new CommodityTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                SupplierRequestsCount = x.SupplierRequests?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: CommodityTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var commodityType = await _repository.GetByIdAsync(id);

            if (commodityType == null)
            {
                return NotFound();
            }

            var model = new CommodityTypesDetailsVM
            {
                Id = commodityType.Id,
                Name = commodityType.Name,
                SupplierRequestsCount = commodityType.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // GET: CommodityTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CommodityTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommodityTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع السلعة مسجل بالفعل.");
                return View(model);
            }

            var commodityType = new CommodityTypes
            {
                Name = model.Name
            };

            await _repository.AddAsync(commodityType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: CommodityTypes/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var commodityType = await _repository.GetByIdAsync(id);

            if (commodityType == null)
            {
                return NotFound();
            }

            var model = new UpdateCommodityTypesVM
            {
                Id = commodityType.Id,
                Name = commodityType.Name
            };

            return View(model);
        }

        // POST: CommodityTypes/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateCommodityTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var commodityType = await _repository.GetByIdAsync(model.Id);

            if (commodityType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع السلعة مسجل بالفعل.");
                return View(model);
            }

            commodityType.Name = model.Name;

            _repository.Update(commodityType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: CommodityTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var commodityType = await _repository.GetByIdAsync(id);

            if (commodityType == null)
            {
                return NotFound();
            }

            var model = new CommodityTypesDetailsVM
            {
                Id = commodityType.Id,
                Name = commodityType.Name,
                SupplierRequestsCount = commodityType.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // POST: CommodityTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commodityType = await _repository.GetByIdAsync(id);

            if (commodityType == null)
            {
                return NotFound();
            }

            _repository.Remove(commodityType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}