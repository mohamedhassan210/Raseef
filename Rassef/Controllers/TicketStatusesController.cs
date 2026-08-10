namespace Rassef.Controllers
{
    public class TicketStatusesController : Controller
    {
        private readonly IRepository<TicketStatuses> _repository;

        public TicketStatusesController(IRepository<TicketStatuses> repository)
        {
            _repository = repository;
        }

        // GET: TicketStatuses
        public async Task<IActionResult> Index()
        {
            var statuses = await _repository.GetAllAsync();

            var model = statuses.Select(x => new TicketStatusesListVM
            {
                Id = x.Id,
                Name = x.Name,
                QueueTicketsCount = x.QueueTickets?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: TicketStatuses/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            var model = new TicketStatusesDetailsVM
            {
                Id = status.Id,
                Name = status.Name,
                QueueTicketsCount = status.QueueTickets?.Count ?? 0
            };

            return View(model);
        }

        // GET: TicketStatuses/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TicketStatuses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة التذكرة مسجل بالفعل.");
                return View(model);
            }

            var ticketStatus = new TicketStatuses
            {
                Name = model.Name
            };

            await _repository.AddAsync(ticketStatus);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TicketStatuses/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            var model = new UpdateTicketStatusesVM
            {
                Id = status.Id,
                Name = status.Name
            };

            return View(model);
        }

        // POST: TicketStatuses/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTicketStatusesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var status = await _repository.GetByIdAsync(model.Id);

            if (status == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم حالة التذكرة مسجل بالفعل.");
                return View(model);
            }

            status.Name = model.Name;

            _repository.Update(status);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TicketStatuses/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            var model = new TicketStatusesDetailsVM
            {
                Id = status.Id,
                Name = status.Name,
                QueueTicketsCount = status.QueueTickets?.Count ?? 0
            };

            return View(model);
        }

        // POST: TicketStatuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            _repository.Remove(status);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}