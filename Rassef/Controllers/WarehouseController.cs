
namespace Rassef.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehousesController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id, CancellationToken cancellationToken)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetWithDetailsByIdAsync(id.Value, cancellationToken);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location")] Warehouse warehouse, CancellationToken cancellationToken)
        {
            bool isUnique = await _warehouseRepository.IsNameUniqueAsync(warehouse.Name, cancellationToken: cancellationToken);
            if (!isUnique)
            {
                ModelState.AddModelError("Name", "اسم المستودع موجود بالفعل.");
            }

            if (ModelState.IsValid)
            {
                await _warehouseRepository.AddAsync(warehouse, cancellationToken);
                await _warehouseRepository.SaveChangesAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value, cancellationToken);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Location")] Warehouse warehouse, CancellationToken cancellationToken)
        {
            if (id != warehouse.Id) return NotFound();

            bool isUnique = await _warehouseRepository.IsNameUniqueAsync(warehouse.Name, excludedId: id, cancellationToken: cancellationToken);
            if (!isUnique)
            {
                ModelState.AddModelError("Name", "اسم المستودع مستخدم بالفعل لمستودع آخر.");
            }

            if (ModelState.IsValid)
            {
                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync(cancellationToken);
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value, cancellationToken);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
            if (warehouse != null)
            {
                _warehouseRepository.Delete(warehouse);
                await _warehouseRepository.SaveChangesAsync(cancellationToken);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}