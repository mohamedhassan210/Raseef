

namespace Rassef.Controllers
{
    public class TruckController : Controller
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IRepository<TruckTypes> _truckTypeRepository;
        private readonly IRepository<User> _userRepository;

        public TruckController(
            ITruckRepository truckRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<User> userRepository)
        {
            _truckRepository = truckRepository;
            _truckTypeRepository = truckTypeRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var trucksrepo = await _truckRepository.GetTruckWithTypeName();

            var trucks = trucksrepo.Select(x => new TruckListVM
            {
                Id = x.Id,
                IsRefrigerated = x.IsRefrigerated,
                PlateLetter = x.PlateLetter,
                PlateNumber = x.PlateNumber,
                StorageCapacity = x.StorageCapacity,
                TruckTypeName = x.TruckType?.Name ?? "غير محدد",
            }).ToList();

            return View(trucks);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("الشاحنة", "رقم الشاحنة مفقود.");
                return View(new TruckDetailsVM());
            }

            var truck = await _truckRepository.GetByIdAsync(id.Value);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                return View(new TruckDetailsVM());
            }

            var truckDetails = new TruckDetailsVM
            {
                Id = truck.Id,
                PlateNumber = truck.PlateNumber,
                PlateLetter = truck.PlateLetter,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeName = truck.TruckType?.Name ?? "غير محدد",
                CreatedByName = truck.CreatedBy?.Name ?? "النظام",
                CreatedAt = truck.CreatedAT,
                UpdatedAt = truck.UpdatedAT
            };

            return View(truckDetails);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadTruckTypesAsync();
            return View(new CreateTruckVM());
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

            var currentUser = await _userRepository.GetByIdAsync(int.Parse(userId));

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
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("الشاحنة", "رقم الشاحنة مفقود.");
                await LoadTruckTypesAsync();
                return View(new UpdateTruckVM());
            }

            var truck = await _truckRepository.GetByIdAsync(id.Value);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                await LoadTruckTypesAsync();
                return View(new UpdateTruckVM());
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTruckVM update)
        {
            if (!ModelState.IsValid)
            {
                await LoadTruckTypesAsync(update.TruckTypeId);
                return View(update);
            }

            var truck = await _truckRepository.GetByIdAsync(update.Id);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("الشاحنة", "رقم الشاحنة مفقود.");
                return View(new TruckDetailsVM());
            }

            var truck = await _truckRepository.GetByIdAsync(id.Value);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                return View(new TruckDetailsVM());
            }

            var vm = new TruckDetailsVM
            {
                Id = truck.Id,
                PlateNumber = truck.PlateNumber,
                PlateLetter = truck.PlateLetter,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeName = truck.TruckType?.Name ?? "غير محدد",
                CreatedByName = truck.CreatedBy?.Name ?? "غير محدد",
                CreatedAt = truck.CreatedAT,
                UpdatedAt = truck.UpdatedAT
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                return View(new TruckDetailsVM());
            }

            _truckRepository.Remove(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الشاحنة بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        #region Helpers
        private async Task LoadTruckTypesAsync(int? selectedTruckTypeId = null)
        {
            ViewBag.TruckTypes = new SelectList(
                await _truckTypeRepository.GetAllAsync(),
                "Id",
                "Name",
                selectedTruckTypeId);
        }
        #endregion
    }
}