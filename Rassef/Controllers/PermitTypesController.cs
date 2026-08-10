

namespace Rassef.Controllers
{
    public class PermitTypesController : Controller
    {
        private readonly IRepository<PermitTypes> _repository;

        public PermitTypesController(IRepository<PermitTypes> repository)
        {
            _repository = repository;
        }

        // GET: PermitTypes
        public async Task<IActionResult> Index()
        {
            var permitTypes = await _repository.GetAllAsync();

            var model = permitTypes.Select(x => new PermitTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                TransferRequestsCount = x.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = x.SupplierRequests?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: PermitTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var permitType = await _repository.GetByIdAsync(id);

            if (permitType == null)
            {
                return NotFound();
            }

            var model = new PermitTypesDetailsVM
            {
                Id = permitType.Id,
                Name = permitType.Name,
                TransferRequestsCount = permitType.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = permitType.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // GET: PermitTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PermitTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePermitTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع التصريح مسجل بالفعل.");
                return View(model);
            }

            var permitType = new PermitTypes
            {
                Name = model.Name
            };

            await _repository.AddAsync(permitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: PermitTypes/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var permitType = await _repository.GetByIdAsync(id);

            if (permitType == null)
            {
                return NotFound();
            }

            var model = new UpdatePermitTypesVM
            {
                Id = permitType.Id,
                Name = permitType.Name
            };

            return View(model);
        }

        // POST: PermitTypes/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdatePermitTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var permitType = await _repository.GetByIdAsync(model.Id);

            if (permitType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع التصريح مسجل بالفعل.");
                return View(model);
            }

            permitType.Name = model.Name;

            _repository.Update(permitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: PermitTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var permitType = await _repository.GetByIdAsync(id);

            if (permitType == null)
            {
                return NotFound();
            }

            var model = new PermitTypesDetailsVM
            {
                Id = permitType.Id,
                Name = permitType.Name,
                TransferRequestsCount = permitType.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = permitType.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // POST: PermitTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permitType = await _repository.GetByIdAsync(id);

            if (permitType == null)
            {
                return NotFound();
            }

            _repository.Remove(permitType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}