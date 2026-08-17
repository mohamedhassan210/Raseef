namespace Rassef.Controllers
{
    public class QueueSettingsController : Controller
    {
        private readonly IRepository<QueueSettings> _repository;
        private readonly IRepository<Shift> _shiftRepository;

        public QueueSettingsController(
            IRepository<QueueSettings> repository,
            IRepository<Shift> shiftRepository)
        {
            _repository = repository;
            _shiftRepository = shiftRepository;
        }

        // Display all items
        public async Task<IActionResult> Index()
        {
            var settings = await _repository.GetAllAsync();
            var shifts = await _shiftRepository.GetAllAsync();

            var model = settings.Select(x => new QueueSettingsListVM
            {
                Id = x.Id,
                ResetType = x.ResetType.ToString(),
                ShiftName = shifts.FirstOrDefault(s => s.Id == x.ShiftId)?.Name
            }).ToList();

            return View(model);
        }

        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var settings = await _repository.GetByIdAsync(id);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات المطلوبة غير موجودة.");
                return View();
            }

            var shift = settings.ShiftId.HasValue
                ? await _shiftRepository.GetByIdAsync(settings.ShiftId.Value)
                : null;

            var model = new QueueSettingsDetailsVM
            {
                Id = settings.Id,
                ResetType = settings.ResetType.ToString(),
                ShiftName = shift?.Name,
                CreatedAT = settings.CreatedAT
            };

            return View(model);
        }

        [HttpGet]
        // Display create page
        public async Task<IActionResult> Create()
        {
            var model = new CreateQueueSettingsVM();
            await PopulateDropdown(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item - BL-7: التحقق من عدم وجود إعدادات سابقة (سجل واحد فقط)
        public async Task<IActionResult> Create(CreateQueueSettingsVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdown(model);
                return View(model);
            }

            // BL-7: منع إنشاء أكثر من سجل QueueSettings واحد في النظام
            var existingSettings = await _repository.FindAsync(x => true);
            if (existingSettings != null)
            {
                TempData["Error"] = "توجد إعدادات بالفعل. قم بتعديل الإعدادات الحالية بدلاً من إنشاء جديدة.";
                return RedirectToAction(nameof(Update), new { id = existingSettings.Id });
            }

            var settings = new QueueSettings
            {
                ResetType = model.ResetType,
                ShiftId = model.ShiftId
            };

            await _repository.AddAsync(settings);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display update page
        public async Task<IActionResult> Update(int id)
        {
            var settings = await _repository.GetByIdAsync(id);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات المطلوب تعديلها غير موجودة.");
                return View();
            }

            var model = new UpdateQueueSettingsVM
            {
                Id = settings.Id,
                ResetType = settings.ResetType,
                ShiftId = settings.ShiftId
            };

            await PopulateDropdown(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Update item
        public async Task<IActionResult> Update(UpdateQueueSettingsVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdown(model);
                return View(model);
            }

            var settings = await _repository.GetByIdAsync(model.Id);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات المطلوب تعديلها غير موجودة.");
                return View(model);
            }

            settings.ResetType = model.ResetType;
            settings.ShiftId = model.ShiftId;

            _repository.Update(settings);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        // Action ResetAll
        public async Task<IActionResult> ResetAll()
        {
            var settings = await _repository.FindAsync(x => true);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات غير موجودة.");
                return RedirectToAction(nameof(Index));
            }

            settings.LastGlobalResetAt = DateTimeOffset.Now;

            _repository.Update(settings);
            await _repository.SaveChangesAsync();

            TempData["Success"] = "تم تصفير جميع العدادات.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var settings = await _repository.GetByIdAsync(id);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات المطلوب حذفها غير موجودة.");
                return View();
            }

            var shift = settings.ShiftId.HasValue
                ? await _shiftRepository.GetByIdAsync(settings.ShiftId.Value)
                : null;

            var model = new QueueSettingsDetailsVM
            {
                Id = settings.Id,
                ResetType = settings.ResetType.ToString(),
                ShiftName = shift?.Name,
                CreatedAT = settings.CreatedAT
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var settings = await _repository.GetByIdAsync(id);

            if (settings == null)
            {
                ModelState.AddModelError("", "الإعدادات المطلوب حذفها غير موجودة.");
                return View();
            }

            _repository.Remove(settings);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdown(CreateQueueSettingsVM model)
        {
            var shifts = await _shiftRepository.GetAllAsync();

            model.Shifts = shifts.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });
        }

        private async Task PopulateDropdown(UpdateQueueSettingsVM model)
        {
            var shifts = await _shiftRepository.GetAllAsync();

            model.Shifts = shifts.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });
        }
    }
}
