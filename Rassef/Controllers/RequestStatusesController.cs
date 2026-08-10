

namespace Rassef.Controllers
{
    public class RequestStatusesController : Controller
    {
        private readonly IRepository<RequestStatuses> _repository;

        public RequestStatusesController(IRepository<RequestStatuses> repository)
        {
            _repository = repository;
        }

        // GET: RequestStatuses
        public async Task<IActionResult> Index()
        {
            var requestStatuses = await _repository.GetAllAsync();

            var model = requestStatuses.Select(x => new RequestStatusesListVM
            {
                Id = x.Id,
                Name = x.Name,
                TransferRequestsCount = x.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = x.SupplierRequests?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: RequestStatuses/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var requestStatus = await _repository.GetByIdAsync(id);

            if (requestStatus == null)
            {
                return NotFound();
            }

            var model = new RequestStatusesDetailsVM
            {
                Id = requestStatus.Id,
                Name = requestStatus.Name,
                TransferRequestsCount = requestStatus.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = requestStatus.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // GET: RequestStatuses/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RequestStatuses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة الطلب مسجل بالفعل.");
                return View(model);
            }

            var requestStatus = new RequestStatuses
            {
                Name = model.Name
            };

            await _repository.AddAsync(requestStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: RequestStatuses/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var requestStatus = await _repository.GetByIdAsync(id);

            if (requestStatus == null)
            {
                return NotFound();
            }

            var model = new UpdateRequestStatusesVM
            {
                Id = requestStatus.Id,
                Name = requestStatus.Name
            };

            return View(model);
        }

        // POST: RequestStatuses/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateRequestStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var requestStatus = await _repository.GetByIdAsync(model.Id);

            if (requestStatus == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة الطلب مسجل بالفعل.");
                return View(model);
            }

            requestStatus.Name = model.Name;

            _repository.Update(requestStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: RequestStatuses/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var requestStatus = await _repository.GetByIdAsync(id);

            if (requestStatus == null)
            {
                return NotFound();
            }

            var model = new RequestStatusesDetailsVM
            {
                Id = requestStatus.Id,
                Name = requestStatus.Name,
                TransferRequestsCount = requestStatus.TransferRequests?.Count ?? 0,
                SupplierRequestsCount = requestStatus.SupplierRequests?.Count ?? 0
            };

            return View(model);
        }

        // POST: RequestStatuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var requestStatus = await _repository.GetByIdAsync(id);

            if (requestStatus == null)
            {
                return NotFound();
            }

            _repository.Remove(requestStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}