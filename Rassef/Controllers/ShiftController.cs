namespace Rassef.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IRepository<Shift> _repository;

        public ShiftController(IRepository<Shift> repository)
        {
            _repository = repository;
        }

        // Display all items
        public async Task<IActionResult> Index()
        {
            var shifts = await _repository.GetAllAsync();

            var model = shifts.Select(x => new ShiftListVM
            {
                Id = x.Id,
                Name = x.Name,
                StartTime = x.StartTime,
                Duration = x.Duration
            }).ToList();

            return View(model);
        }

        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب غير موجود.");
                return View();
            }

            var model = new ShiftDetailsVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                Duration = shift.Duration,
                CreatedAT = shift.CreatedAT
            };

            return View(model);
        }

        [HttpGet]
        // Display create page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item
        public async Task<IActionResult> Create(CreateShiftVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الشيفت مسجل بالفعل.");
                return View(model);
            }

            var shift = new Shift
            {
                Name = model.Name,
                StartTime = model.StartTime,
                Duration = model.Duration
            };

            await _repository.AddAsync(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display update page
        public async Task<IActionResult> Update(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب تعديله غير موجود.");
                return View();
            }

            var model = new UpdateShiftVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                Duration = shift.Duration
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Update item
        public async Task<IActionResult> Update(UpdateShiftVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var shift = await _repository.GetByIdAsync(model.Id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب تعديله غير موجود.");
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الشيفت مسجل بالفعل.");
                return View(model);
            }

            shift.Name = model.Name;
            shift.StartTime = model.StartTime;
            shift.Duration = model.Duration;

            _repository.Update(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب حذفه غير موجود.");
                return View();
            }

            var model = new ShiftDetailsVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                Duration = shift.Duration,
                CreatedAT = shift.CreatedAT
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب حذفه غير موجود.");
                return View();
            }

            _repository.Remove(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        //Reset
        [HttpPost]
        public async Task<IActionResult> Reset(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت غير موجود.");
                return RedirectToAction(nameof(Index));
            }

            shift.LastResetAt = DateTimeOffset.Now;

            _repository.Update(shift);
            await _repository.SaveChangesAsync();

            TempData["Success"] = "تم تصفير الشيفت.";

            return RedirectToAction(nameof(Index));
        }
    }
}
