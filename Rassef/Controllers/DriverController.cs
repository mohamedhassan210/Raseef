using Microsoft.AspNetCore.Mvc;
using Rassef.ViewModels.Driver;
using System.Security.Claims;

namespace Rassef.Controllers
{
    public class DriverController : Controller
    {
        private readonly DriverRepository _repository;

        public DriverController(DriverRepository driverRepository)
        {
            _repository = driverRepository;
        }

        // Get All Drivers
        public async Task<IActionResult> Index()
        {
            var drivers = await _repository.GetAllAsync();

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
        public async Task<IActionResult> Details(Guid id)
        {
            var drv = await _repository.GetByIdAsync(id);

            if (drv is null)
                return NotFound();

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
        public IActionResult Create()
        {
            return View();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverVM create)
        {
            if (!ModelState.IsValid)
                return View(create);

            if (await _repository.ExistsAsync(x => x.NationalId == create.NationalId))
            {
                ModelState.AddModelError(nameof(create.NationalId), "الرقم القومي مسجل بالفعل.");
                return View(create);
            }

            if (await _repository.ExistsAsync(x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(nameof(create.Phone), "رقم الهاتف مسجل بالفعل.");
                return View(create);
            }

            var driver = new Driver
            {
                FullName = create.FullName,
                NationalId = create.NationalId,
                Phone = create.Phone,
                CreatedById = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            };

            await _repository.AddAsync(driver);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Update (GET)
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var drv = await _repository.GetByIdAsync(id);

            if (drv is null)
                return NotFound();

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
                return View(updateDriverVM);

            var driver = await _repository.GetByIdAsync(updateDriverVM.Id);

            if (driver is null)
                return NotFound();

            if (await _repository.ExistsAsync(x => x.NationalId == updateDriverVM.NationalId && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.NationalId), "الرقم القومي مسجل بالفعل.");
                return View(updateDriverVM);
            }

            if (await _repository.ExistsAsync(x => x.Phone == updateDriverVM.Phone && x.Id != updateDriverVM.Id))
            {
                ModelState.AddModelError(nameof(updateDriverVM.Phone), "رقم الهاتف مسجل بالفعل.");
                return View(updateDriverVM);
            }

            driver.FullName = updateDriverVM.FullName;
            driver.NationalId = updateDriverVM.NationalId;
            driver.Phone = updateDriverVM.Phone;

            _repository.Update(driver);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var drv = await _repository.GetByIdAsync(id);

            if (drv is null)
                return NotFound();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(Guid id)
        {
            var driver = await _repository.GetByIdAsync(id);

            if (driver is null)
                return NotFound();

            if (await _repository.HasRequestsAsync(driver.Id))
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

            _repository.Remove(driver);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}