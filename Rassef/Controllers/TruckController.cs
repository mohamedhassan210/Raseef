namespace Rassef.Controllers
{
    public class TruckController : Controller
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<TruckTypes> _truckTypeRepository;
        private readonly IRepository<User> _userRepository;
        public TruckController(IRepository<Truck> truckRepository, IRepository<TruckTypes> truckTypeRepository , IRepository<User> userRepository)
        {
            _truckRepository = truckRepository;
            _truckTypeRepository = truckTypeRepository;
            _userRepository = userRepository;
        } 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trucksrepo = await _truckRepository.GetAllAsync();
            if (!trucksrepo.Any())
            {
                return View(trucksrepo);
            }
            var trucks = trucksrepo.Select(x => new TruckListVM
            {
                Id = x.Id,
                IsRefrigerated = x.IsRefrigerated,
                PlateLetter = x.PlateLetter,
                PlateNumber = x.PlateNumber,
                StorageCapacity = x.StorageCapacity,
                TruckTypeName = x.TruckType.Name

            });
            return View(trucks);
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);
            if (truck == null) return View(truck);
            var truckDetails = new TruckDetailsVM
            {
                PlateNumber = truck.PlateNumber,
                PlateLetter = truck.PlateLetter,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeName = truck.TruckType.Name,
                CreatedByName = truck.CreatedBy.Name,
                CreatedAt = DateTime.UtcNow
            };
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var truckTypes = await _truckTypeRepository.GetAllAsync();

            ViewBag.TruckTypes = new SelectList(truckTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTruckVM create)
        {
            if (!ModelState.IsValid)
            {
                await LoadTruckTypesAsync(create.TruckTypeId);
                return View(create);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError("", "يجب تسجيل الدخول أولاً.");

                await LoadTruckTypesAsync(create.TruckTypeId);
                return View(create);
            }

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(userId));

            if (currentUser is null)
            {
                ModelState.AddModelError("", "لم يتم العثور على المستخدم.");

                await LoadTruckTypesAsync(create.TruckTypeId);
                return View(create);
            }

            var truck = new Truck
            {
                PlateNumber = create.PlateNumber,
                PlateLetter = create.PlateLetter,
                StorageCapacity = create.StorageCapacity,
                IsRefrigerated = create.IsRefrigerated,
                TruckTypeId = create.TruckTypeId,
                CreatedBy = currentUser
            };

            await _truckRepository.AddAsync(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الشاحنة بنجاح.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck is null)
                return NotFound();

            await LoadTruckTypesAsync(truck.TruckTypeId);

            var vm = new UpdateTruckVM
            {
                Id = truck.Id,
                PlateNumber = truck.PlateNumber,
                PlateLetter = truck.PlateLetter,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeId = truck.TruckTypeId
            };

            return View(vm);
        }
        [HttpPut]
        public async Task<IActionResult> Update(UpdateTruckVM update)
        {
            if (!ModelState.IsValid)
            {
                await LoadTruckTypesAsync(update.TruckTypeId);
                return View(update);
            }
            var truck = await _truckRepository.GetByIdAsync(update.Id);

            if (truck is null)
            {
                ModelState.AddModelError("", "الشاحنة غير موجودة.");

                await LoadTruckTypesAsync(update.TruckTypeId);
                return View(update);
            }
            truck.PlateNumber = update.PlateNumber;
            truck.PlateLetter = update.PlateLetter;
            truck.StorageCapacity = update.StorageCapacity;
            truck.IsRefrigerated = update.IsRefrigerated;
            truck.TruckTypeId = update.TruckTypeId;

            _truckRepository.Update(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات الشاحنة بنجاح.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck is null)
                return NotFound();

            var vm = new TruckDetailsVM
            {
                Id = truck.Id,
                PlateNumber = truck.PlateNumber,
                PlateLetter = truck.PlateLetter,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeName = truck.TruckType.Name,
                CreatedByName = truck.CreatedBy.Name,
                CreatedAt = truck.CreatedAT,
                UpdatedAt = truck.UpdatedAT
            };

            return View(vm);
        }
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck is null)
            {
                TempData["Error"] = "الشاحنة غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

            _truckRepository.Remove(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الشاحنة بنجاح.";

            return RedirectToAction(nameof(Index));
        }
        // Helpers 
        private async Task LoadTruckTypesAsync(Guid? selectedTruckTypeId = null)
        {
            ViewBag.TruckTypes = new SelectList(
                await _truckTypeRepository.GetAllAsync(),
                "Id",
                "Name",
                selectedTruckTypeId);
        }

    }
}
