namespace Rassef.Controllers
{
    public class DriverController : Controller
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ISupplierRepository _supplierRepository;

        public DriverController(IDriverRepository repository, ISupplierRepository supplierRepository)
        {
            _driverRepository = repository;
            _supplierRepository = supplierRepository;
        }

        // Get All Drivers
        public async Task<IActionResult> Index()
        {
            var drivers = await _driverRepository.GetAllAsync();

            var driverList = drivers.Select(d => new DriverListVM
            {
                Id = d.Id,
                FullName = d.FullName,
                NationalId = d.NationalId,
                Phone = d.Phone
            }).ToList();

            return View(driverList);
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
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(nameof(create.Phone), "رقم الهاتف مسجل بالفعل.");
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

            return RedirectToAction(nameof(Index));
        }

        // Update (GET)
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var drv = await _driverRepository.GetByIdAsync(id);

            if (drv is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return View();
            }

            var driver = new UpdateDriverVM
            {
                Id = drv.Id,
                FullName = drv.FullName,
                NationalId = drv.NationalId,
                Phone = drv.Phone
            };

            return View(driver);
        }

        // Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDriverVM updateDriverVM)
        {
            if (!ModelState.IsValid)
            {
                return View(updateDriverVM);
            }

            var driver = await _driverRepository.GetByIdAsync(updateDriverVM.Id);

            if (driver is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return View(updateDriverVM);
            }

            if (await _driverRepository.ExistsAsync(x => x.NationalId == updateDriverVM.NationalId && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.NationalId), "الرقم القومي مسجل بالفعل.");
                return View(updateDriverVM);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == updateDriverVM.Phone && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.Phone), "رقم الهاتف مسجل بالفعل.");
                return View(updateDriverVM);
            }

            driver.FullName = updateDriverVM.FullName;
            driver.NationalId = updateDriverVM.NationalId;
            driver.Phone = updateDriverVM.Phone;

            _driverRepository.Update(driver);
            await _driverRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);

            if (driver is null)
            {
                ModelState.AddModelError("", "السائق غير موجود.");
                return RedirectToAction(nameof(Index));
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
    }
}