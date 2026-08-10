using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rassef.Common.Interfaces.Services.AuthenticationServices;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.ValueObjects;
using Rassef.ViewModels.Administration;
using Rassef.ViewModels.Administration.Employee;
using Rassef.ViewModels.Administration.Truck;
namespace Rassef.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Position> _positionRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<TruckTypes> _truckTypesRepository;

        public AdministrationController(
            IUserRepository userRepository,
            IRepository<Position> positionRepository,
            IRepository<Truck> truckRepository,
                        IRepository<TruckTypes> truckTypeRepository)
        {
            _userRepository = userRepository;
            _positionRepository = positionRepository;
            _truckRepository = truckRepository;
            _truckTypesRepository = truckTypeRepository;

            
        }
        //Employee Administration
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Where(u => !u.IsDeleted).Include(u => u.Position)
            );

            var employees = users.Select(u => new EmployeeListVM
            {
                Id = u.Id,
                Name = u.Name,
                Phone = u.Phone,
                Email = u.Email?.ToString() ?? string.Empty,
                Role = u.Position?.PositionName ?? "غير محدد",
                NationalId = u.NationalId,
                UserCode = u.UserCode ?? string.Empty
            }).ToList();

            if (!employees.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "لا يوجد أي موظفين حتى الآن."
                );
            }

            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var employee = new EmployeeDetailsVM
            {
                Id = user.Id,
                Name = user.Name,
                UserCode = user.UserCode ?? string.Empty,
                Role = user.Position?.PositionName ?? "غير محدد",
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                NationalId = user.NationalId
            };

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var positions = await _positionRepository.GetAllAsync();

            ViewBag.Positions = positions;

            return View(new EmployeeCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync();
                ViewBag.Positions = positions;

                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Phone = model.Phone,
                Email = Email.Create(model.Email),
                PositionId = model.PositionId,
                NationalId = model.NationalId,
                UserCode = model.UserCode
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var positions = await _positionRepository.GetAllAsync();

            ViewBag.Positions = positions;

            var employee = new EmployeeEditVM
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                PositionId = user.PositionId,
                NationalId = user.NationalId,
                UserCode = user.UserCode ?? string.Empty
            };

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditVM model)
        {
            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync();
                ViewBag.Positions = positions;

                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(model.Id);

            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return View(model);
            }

            user.Name = model.Name;
            user.Phone = model.Phone;
            user.Email = Email.Create(model.Email);
            user.PositionId = model.PositionId;
            user.NationalId = model.NationalId;
            user.UserCode = model.UserCode;
            user.MarkAsUpdated();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم تعديل بيانات الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var employee = new EmployeeDeleteVM
            {
                Id = user.Id,
                Name = user.Name,
                UserCode = user.UserCode ?? string.Empty,
                Role = user.Position?.PositionName ?? "غير محدد",
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                NationalId = user.NationalId
            };

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            user.IsDeleted = true;
            user.MarkAsUpdated();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        ////////////////////
        ////////////////////

        //Truck Administration

        [HttpGet]
        public async Task<IActionResult> TrucksIndex()
        {
            var trucks = await _truckRepository.GetAllAsync(query =>
                query.Where(t => !t.IsDeleted)
                     .Include(t => t.CreatedBy)
                     .Include(t => t.TruckType)
            );

            var truckList = trucks.Select(t => new ViewModels.Administration.TruckListVM
            {
                Id = t.Id,
                PlateNumber = $"{t.PlateLetter} {t.PlateNumber}",
                IsRefrigerated = t.IsRefrigerated ? "تبريد" : "لا تبريد",
                Company = t.TruckType?.Name ?? "غير محدد",
                StorageCapacity = t.StorageCapacity,
                HostEmployeeName = t.CreatedBy?.Name ?? "غير محدد"
            }).ToList();

            if (!truckList.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "لا يوجد أي شاحنات مسجلة حتى الآن."
                );
            }

            return View(truckList);
        }

        [HttpGet]
        public async Task<IActionResult> TruckDetails(int id)
        {
            var trucks = await _truckRepository.GetAllAsync(query =>
                query.Where(t => !t.IsDeleted)
                     .Include(t => t.CreatedBy)
                     .Include(t => t.TruckType)
                     .Include(t => t.SupplierRequests)
                     .Include(t => t.TransferRequests)
            );

            var truck = trucks.FirstOrDefault(t => t.Id == id);

            if (truck == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذه الشاحنة غير موجودة."
                );

                return RedirectToAction(nameof(TrucksIndex));
            }

            int visitsCount = (truck.SupplierRequests?.Count ?? 0) + (truck.TransferRequests?.Count ?? 0);

            var truckDetails = new ViewModels.Administration.Truck.TruckDetailsVM
            {
                Id = truck.Id,
                VisitsCount = visitsCount,
                PlateNumber = $"{truck.PlateLetter} {truck.PlateNumber}",
                IsRefrigerated = truck.IsRefrigerated ? "تبريد" : "لا تبريد",
                Company = truck.TruckType?.Name ?? "غير محدد",
                StorageCapacity = truck.StorageCapacity,
                HostEmployeeName = truck.CreatedBy?.Name ?? "غير محدد"
            };

            return View(truckDetails);
        }

        [HttpGet]
        public async Task<IActionResult> TruckCreate()
        {
            var truckTypes = await _truckTypesRepository.GetAllAsync();
            ViewBag.TruckTypes = truckTypes;

            return View(new TruckCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TruckCreate(TruckCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var truckTypes = await _truckTypesRepository.GetAllAsync();
                ViewBag.TruckTypes = truckTypes;

                return View(model);
            }

            // جلب معرف الموظف الحالي من الـ Claims
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var truck = new Truck
            {
                PlateLetter = model.PlateLetter,
                PlateNumber = model.PlateNumber,
                StorageCapacity = model.StorageCapacity,
                IsRefrigerated = model.IsRefrigerated,
                TruckTypeId = model.TruckTypeId,
                CreatedById = currentUserId
            };

            await _truckRepository.AddAsync(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الشاحنة بنجاح.";

            return RedirectToAction(nameof(TrucksIndex));
        }

        [HttpGet]
        public async Task<IActionResult> TruckEdit(int id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck == null || truck.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذه الشاحنة غير موجودة."
                );

                return RedirectToAction(nameof(TrucksIndex));
            }

            var truckTypes = await _truckTypesRepository.GetAllAsync();
            ViewBag.TruckTypes = truckTypes;

            var model = new TruckEditVM
            {
                Id = truck.Id,
                PlateLetter = truck.PlateLetter,
                PlateNumber = truck.PlateNumber,
                StorageCapacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                TruckTypeId = truck.TruckTypeId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TruckEdit(TruckEditVM model)
        {
            if (!ModelState.IsValid)
            {
                var truckTypes = await _truckTypesRepository.GetAllAsync();
                ViewBag.TruckTypes = truckTypes;

                return View(model);
            }

            var truck = await _truckRepository.GetByIdAsync(model.Id);

            if (truck == null || truck.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذه الشاحنة غير موجودة."
                );

                return RedirectToAction(nameof(TrucksIndex));
            }

            truck.PlateLetter = model.PlateLetter;
            truck.PlateNumber = model.PlateNumber;
            truck.StorageCapacity = model.StorageCapacity;
            truck.IsRefrigerated = model.IsRefrigerated;
            truck.TruckTypeId = model.TruckTypeId;
            truck.MarkAsUpdated();

            _truckRepository.Update(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم تعديل بيانات الشاحنة بنجاح.";

            return RedirectToAction(nameof(TrucksIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TruckDeleteConfirmed(int id)
        {
            var truck = await _truckRepository.GetByIdAsync(id);

            if (truck == null || truck.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذه الشاحنة غير موجودة."
                );

                return RedirectToAction(nameof(TrucksIndex));
            }

            // Soft Delete
            truck.IsDeleted = true;
            truck.MarkAsUpdated();

            _truckRepository.Update(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الشاحنة بنجاح.";

            return RedirectToAction(nameof(TrucksIndex));
        }
    }
}