using AspNetCoreGeneratedDocument;

namespace Rassef.Controllers
{
    public class TruckController : Controller
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IRepository<TruckTypes> _truckTypeRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IRepository<TruckTypes> _truckTypes;



        public TruckController(
            ITruckRepository truckRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<User> userRepository,
             ISupplierRepository supplierRepository,
             IDriverRepository driverRepository,
             IRepository<TruckTypes> truckTypes)
        {
            _truckRepository = truckRepository;
            _truckTypeRepository = truckTypeRepository;
            _userRepository = userRepository;
            _supplierRepository = supplierRepository;
            _driverRepository = driverRepository;
            _truckTypes = truckTypes;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int supplierid, string searchString)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierid);
            if (supplier is null)
            {
                return RedirectToAction("Index", "Supplier");
            }

            var trucksrepo = await _truckRepository.GetTruckWithTypeName();

            var supplierTrucks = trucksrepo
                .Where(x => x.SupplierRequests.Any(sr => sr.SupplierId == supplierid))
                .Select(x => new TruckListVM
                {
                    Id = x.Id,
                    IsRefrigerated = x.IsRefrigerated,
                    PlateLetter = x.PlateLetter,
                    PlateNumber = x.PlateNumber,
                    StorageCapacity = x.StorageCapacity,
                    TruckTypeName = x.TruckType?.Name ?? "غير محدد",
                    supplierId = supplierid
                });

            // 👇 فلترة البيانات لو المستخدم كتاب حاجة في خانة البحث
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                supplierTrucks = supplierTrucks.Where(x =>
                    (!string.IsNullOrEmpty(x.PlateNumber) && x.PlateNumber.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(x.PlateLetter) && x.PlateLetter.ToLower().Contains(searchString))
                );
            }

            ViewBag.SupplierName = supplier.Name;
            ViewBag.SupplierId = supplier.Id;
            ViewBag.Search = searchString; // لحفظ الكلمة المكتوبة في خانة البحث متبوعة على الشاشة

            return View(supplierTrucks.ToList());
        }
        [HttpGet]
        // Display details
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
        // Display create page
        [HttpGet]
        public async Task<IActionResult> Create(int supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (supplier == null)
            {
                return RedirectToAction("Index", "Supplier");
            }

            // تحميل أنواع الشاحنات في ViewBag.TruckTypes
            await LoadTruckTypesAsync();

            var model = new CreateTruckVM
            {
                SupplierId = supplierId,
                SupplierName = supplier.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item
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
        // Display update page
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
        // Update item
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
        // Display delete confirmation
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
        // Delete item
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

        private async Task<IEnumerable<SelectListItem>> GetSuppliersAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();

            return suppliers.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> MainTraDrivers()
        {

            var allTrucks = await _truckRepository.GetAllAsync();

            var truckList = allTrucks.Select(d => new TruckListVM
            {
                Id = d.Id,
                PlateNumber = d.PlateNumber,
                PlateLetter = d.PlateLetter,
                StorageCapacity = d.StorageCapacity,
                IsRefrigerated = d.IsRefrigerated
            }).ToList();

            return View(truckList);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> AddTraDriver()
        {
            var model = new CreateDriverVM
            {
                Suppliers = await GetSuppliersAsync()
            };

            return View(model);
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTraDriver(CreateDriverVM create)
        {
            if (!ModelState.IsValid)
            {
                create.Suppliers = await GetSuppliersAsync();
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.NationalId == create.NationalId))
            {
                ModelState.AddModelError(nameof(create.NationalId), "الرقم القومي مسجل بالفعل.");
                create.Suppliers = await GetSuppliersAsync();
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(nameof(create.Phone), "رقم الهاتف مسجل بالفعل.");
                create.Suppliers = await GetSuppliersAsync();
                return View(create);
            }

            var driver = new Driver
            {
                FullName = create.FullName,
                NationalId = create.NationalId,
                Phone = create.Phone,
                CreatedById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { supplierId = create.SupplierId });
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
