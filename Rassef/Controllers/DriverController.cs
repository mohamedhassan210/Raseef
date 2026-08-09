
namespace Rassef.Controllers
{
    public class DriverController : Controller
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ITruckRepository _truckRepository;

        public DriverController(IDriverRepository repository, ISupplierRepository supplierRepository, ITruckRepository truckRepository)
        {
            _driverRepository = repository;
            _supplierRepository = supplierRepository;
            _truckRepository = truckRepository;
        }

        // Get All Drivers
        [HttpGet]
        public async Task<IActionResult> Index(int id, int supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (supplier is null) return RedirectToAction("Index", "Supplier");

            var truck = await _truckRepository.GetByIdAsync(id);

            var drivers = await _driverRepository.GetDriversBySupplierIdAsync(supplierId);

            var driverList = drivers.Select(d => new DriverListVM
            {
                Id = d.Id,
                FullName = d.FullName,
                NationalId = d.NationalId,
                Phone = d.Phone
            }).ToList();

            ViewBag.SupplierName = supplier.Name;
            ViewBag.TruckName = truck != null ? $"{truck.PlateLetter} {truck.PlateNumber}" : "سيارة غير محددة";
            ViewBag.SupplierId = supplierId;

            return View(driverList);
        }

        [HttpGet]
        public async Task<IActionResult> Recript()
        {
            return View();
        }

        // Get Driver By Id
        public async Task<IActionResult> Details(int id)
        {
            var drv = await _driverRepository.GetByIdAsync(id);

            if (drv is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return View();
            }

            var driver = new DriverDetailsVM
            {
                Id = drv.Id,
                FullName = drv.FullName,
                NationalId = drv.NationalId,
                Phone = drv.Phone,
                CreatedBy = drv.CreatedById
            };

            return View(driver);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
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
        public async Task<IActionResult> Create(CreateDriverVM create)
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

        // GET: Driver/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int? id, int? supplierId)
        {
            if (id is null || id == 0) return RedirectToAction("Index", "Supplier");

            var drv = await _driverRepository.GetDriverWithRequestsAsync(id.Value);
            if (drv is null) return RedirectToAction("Index", "Supplier");

            int? targetSupplierId = supplierId ?? drv.SupplierRequests?.Select(sr => sr.SupplierId).FirstOrDefault();

            var driverVM = new UpdateDriverVM
            {
                Id = drv.Id,
                FullName = drv.FullName,
                NationalId = drv.NationalId,
                Phone = drv.Phone,
                SupplierId = targetSupplierId,
                Suppliers = await GetSuppliersAsync() // جلب قائمة الشركات
            };

            return View(driverVM);
        }
        // POST: Driver/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDriverVM updateDriverVM)
        {
            var driver = await _driverRepository.GetDriverWithRequestsAsync(updateDriverVM.Id);

            if (driver is null) return RedirectToAction("Index", "Supplier");

            if (!ModelState.IsValid)
            {
                updateDriverVM.Suppliers = await GetSuppliersAsync();
                return View(updateDriverVM);
            }

            if (await _driverRepository.ExistsAsync(x => x.NationalId == updateDriverVM.NationalId && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.NationalId), "الرقم القومي مسجل بالفعل.");
                updateDriverVM.Suppliers = await GetSuppliersAsync();
                return View(updateDriverVM);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == updateDriverVM.Phone && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.Phone), "رقم الهاتف مسجل بالفعل.");
                updateDriverVM.Suppliers = await GetSuppliersAsync();
                return View(updateDriverVM);
            }

            driver.FullName = updateDriverVM.FullName;
            driver.NationalId = updateDriverVM.NationalId;
            driver.Phone = updateDriverVM.Phone;

            _driverRepository.Update(driver);
            await _driverRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { supplierId = updateDriverVM.SupplierId });
        }
        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var drv = await _driverRepository.GetByIdAsync(id);

            if (drv is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return View();
            }

            var driver = new DriverDetailsVM
            {
                Id = drv.Id,
                FullName = drv.FullName,
                NationalId = drv.NationalId,
                Phone = drv.Phone
            };

            return View(driver);
        }

        // Delete (POST)
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(int id, int? supplierId)
        {
            var driver = await _driverRepository.GetByIdAsync(id);

            if (driver is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return RedirectToAction("Index", "Supplier");
            }

            if (await _driverRepository.HasRequestsAsync(driver.Id))
            {
                ModelState.AddModelError("", "لا يمكن حذف السائق لأنه مرتبط بطلبات.");

                var driverVM = new DriverDetailsVM
                {
                    Id = driver.Id,
                    FullName = driver.FullName,
                    NationalId = driver.NationalId,
                    Phone = driver.Phone
                };

                return View("Delete", driverVM);
            }

            _driverRepository.Remove(driver);
            await _driverRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { supplierId = supplierId });
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
    }
}