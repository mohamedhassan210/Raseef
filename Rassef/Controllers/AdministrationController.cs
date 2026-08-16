using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Rassef.ViewModels.Administration;
using Rassef.ViewModels.Administration.Employee;
namespace Rassef.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly ISupplierRequestRepository _supplierRequestRepository;
        private readonly ITransferRequestRepository _Transferrepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Position> _positionRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<TruckTypes> _truckTypesRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverTypes> _driverTypeRepository;
        private readonly IRepository<Supplier> _supplierRepository;

        public AdministrationController(
            IUserRepository userRepository,
            IRepository<Position> positionRepository,
            IRepository<Truck> truckRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverTypes> driverTypeRepository , ITransferRequestRepository transferRequestRepository, ISupplierRequestRepository supplierRequestRepository, IRepository<Supplier> supplierRepository)
        {
            _userRepository = userRepository;
            _positionRepository = positionRepository;
            _truckRepository = truckRepository;
            _truckTypesRepository = truckTypeRepository;
            _driverRepository = driverRepository;
            _driverTypeRepository = driverTypeRepository;
            _Transferrepository = transferRequestRepository;
            _supplierRequestRepository = supplierRequestRepository;
            _supplierRepository = supplierRepository;
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
            //var users = await _userRepository.GetAllAsync(query =>
            //    query.Include(u => u.Position)
            //);

            //var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            //if (user == null)
            //{
            //    ModelState.AddModelError(
            //        string.Empty,
            //        "هذا الموظف غير موجود."
            //    );

            //    return RedirectToAction(nameof(Index));
            //}

            //var employee = new EmployeeDetailsVM
            //{
            //    Id = user.Id,
            //    Name = user.Name,
            //    UserCode = user.UserCode ?? string.Empty,
            //    Role = user.Position?.PositionName ?? "غير محدد",
            //    Phone = user.Phone,
            //    Email = user.Email?.ToString() ?? string.Empty,
            //    NationalId = user.NationalId
            //};

            //return View(employee);
            return View();
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
                UserCode = model.UserCode,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(model.UserCode)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //var users = await _userRepository.GetAllAsync(query =>
            //    query.Include(u => u.Position)
            //);

            //var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            //if (user == null)
            //{
            //    ModelState.AddModelError(
            //        string.Empty,
            //        "هذا الموظف غير موجود."
            //    );

            //    return RedirectToAction(nameof(Index));
            //}

            //var positions = await _positionRepository.GetAllAsync();

            //ViewBag.Positions = positions;

            //var employee = new EmployeeEditVM
            //{
            //    Id = user.Id,
            //    Name = user.Name,
            //    Phone = user.Phone,
            //    Email = user.Email?.ToString() ?? string.Empty,
            //    PositionId = user.PositionId,
            //    NationalId = user.NationalId,
            //    UserCode = user.UserCode ?? string.Empty
            //};

            //return View(employee);
            return View();
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


        ////////////////////
        ////////////////////

        //Driver Administration

        [HttpGet]
        public async Task<IActionResult> DriversIndex()
        {
            var drivers = await _driverRepository.GetAllAsync(query =>
                query
                    .Include(d => d.DeiverType)
                    .Include(d => d.CreatedBy)
            );

            var driverList = drivers.Select(d => new ViewModels.Administration.DriverListVM
            {
                Id = d.Id,
                FullName = d.FullName,
                Phone = d.Phone,
                NationalId = d.NationalId,
                DriverType = d.DeiverType?.Name ?? "غير محدد",
                CreatedBy = d.CreatedBy?.Name ?? "غير محدد"
            }).ToList();

            if (!driverList.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "لا يوجد أي سائقين حتى الآن."
                );
            }

            return View(driverList);
        }


        [HttpGet]
        public async Task<IActionResult> DriverDetails(int id)
        {
            var drivers = await _driverRepository.GetAllAsync(query =>
                query
                    .Include(d => d.DeiverType)
                    .Include(d => d.TransferRequests)
                    .Include(d => d.SupplierRequests)
            );

            var driver = drivers.FirstOrDefault(d => d.Id == id);

            if (driver == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا السائق غير موجود."
                );

                return RedirectToAction(nameof(DriversIndex));
            }

            int visitsCount =
                (driver.TransferRequests?.Count ?? 0) +
                (driver.SupplierRequests?.Count ?? 0);

            var driverDetails = new ViewModels.Administration.DriverDetailsVM
            {
                Id = driver.Id,
                FullName = driver.FullName,
                DriverType = driver.DeiverType?.Name ?? "غير محدد",
                Phone = driver.Phone,
                NationalId = driver.NationalId,
                VisitsCount = visitsCount
            };

            return View(driverDetails);
        }


        [HttpGet]
        public async Task<IActionResult> DriverCreate()
        {
            var driverTypes = await _driverTypeRepository.GetAllAsync();

            ViewBag.DriverTypes = driverTypes;

            return View(new DriverCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DriverCreate(DriverCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var driverTypes = await _driverTypeRepository.GetAllAsync();

                ViewBag.DriverTypes = driverTypes;

                return View(model);
            }

            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var driver = new Driver
            {
                FullName = model.FullName,
                Phone = model.Phone,
                NationalId = model.NationalId,
                DeiverTypeId = model.DeiverTypeId,
                CreatedById = currentUserId
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة السائق بنجاح.";

            return RedirectToAction(nameof(DriversIndex));
        }


        [HttpGet]
        public async Task<IActionResult> DriverEdit(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);

            if (driver == null || driver.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا السائق غير موجود."
                );

                return RedirectToAction(nameof(DriversIndex));
            }

            var driverTypes = await _driverTypeRepository.GetAllAsync();

            ViewBag.DriverTypes = driverTypes;

            var model = new DriverEditVM
            {
                Id = driver.Id,
                FullName = driver.FullName,
                Phone = driver.Phone,
                NationalId = driver.NationalId,
                DeiverTypeId = driver.DeiverTypeId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DriverEdit(DriverEditVM model)
        {
            if (!ModelState.IsValid)
            {
                var driverTypes = await _driverTypeRepository.GetAllAsync();

                ViewBag.DriverTypes = driverTypes;

                return View(model);
            }

            var driver = await _driverRepository.GetByIdAsync(model.Id);

            if (driver == null || driver.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا السائق غير موجود."
                );

                return RedirectToAction(nameof(DriversIndex));
            }

            driver.FullName = model.FullName;
            driver.Phone = model.Phone;
            driver.NationalId = model.NationalId;
            driver.DeiverTypeId = model.DeiverTypeId;

            driver.MarkAsUpdated();

            _driverRepository.Update(driver);
            await _driverRepository.SaveChangesAsync();

            TempData["Success"] = "تم تعديل بيانات السائق بنجاح.";

            return RedirectToAction(nameof(DriversIndex));
        }


        [HttpGet]
        public async Task<IActionResult> DriverDelete(int id)
        {
            var drivers = await _driverRepository.GetAllAsync(query =>
                query
                    .Include(d => d.DeiverType)
            );

            var driver = drivers.FirstOrDefault(d => d.Id == id && !d.IsDeleted);

            if (driver == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا السائق غير موجود."
                );

                return RedirectToAction(nameof(DriversIndex));
            }

            var model = new DriverDeleteVM
            {
                Id = driver.Id,
                FullName = driver.FullName,
                Phone = driver.Phone,
                NationalId = driver.NationalId,
                DriverType = driver.DeiverType?.Name ?? "غير محدد"
            };

            return View(model);
        }

        [HttpPost, ActionName("DriverDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DriverDeleteConfirmed(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);

            if (driver == null || driver.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا السائق غير موجود."
                );

                return RedirectToAction(nameof(DriversIndex));
            }

            driver.IsDeleted = true;
            driver.MarkAsUpdated();

            _driverRepository.Update(driver);
            await _driverRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف السائق بنجاح.";

            return RedirectToAction(nameof(DriversIndex));
        }
        // طلبات التحويل - Index
        public async Task<IActionResult> TransferRequests()
        {
            var requestsRepo =
              await _Transferrepository.GetAllWithDetailsAsync();

            var requests = requestsRepo
            .SelectMany(x => x.QueueTickets.Select(ticket => new TransferRequestListVM
            {
                Id = x.Id,
                TicketNumber = ticket.TicketNumber,
                AvizNumber = x.AvizNumber,
                DateTime = ticket.QueueTime,
                DepartmentName = x.Department.Name ?? "غير محدد",
                DriverName = x.Driver.FullName ?? "غير محدد",
                EmployeeName =
                    x.CreatedBy?.Name ?? "غير محدد",
                TruckPlateNumber =
                    $"{x.Truck?.PlateLetter} {x.Truck?.PlateNumber}",
                RequestStatus = x.RequestStatus.Name,
                DockName = ticket.DockAssignments
                    .OrderByDescending(x => x.AssignedAt)
                    .Select(x => x.Dock.DockName)
                    .FirstOrDefault() ?? "غير محدد",

            }))
            .ToList();

            return View(requests);
        }
        [HttpGet]
        // طلبات التوريد - Index
        public async Task<IActionResult> SupplierRequests()
        {
            var requestsRepo =
                await _supplierRequestRepository.GetAllWithDetailsAsync();

            var requests = requestsRepo
            .SelectMany(x => x.QueueTickets.Select(ticket => new SupplierRequestListVM
            {
                Id = x.Id,
                TicketNumber = ticket.TicketNumber,
                TicketStatusName =
                    ticket.TicketStatus?.Name ?? "غير محدد",
                QueueTime = ticket.QueueTime,
                DockName = ticket.DockAssignments
                    .OrderByDescending(x => x.AssignedAt)
                    .Select(x => x.Dock.DockName)
                    .FirstOrDefault() ?? "غير محدد",
                SupplierName =
                    x.Supplier?.Name ?? "غير محدد",
                TruckPlateNumber =
                    $"{x.Truck?.PlateLetter} {x.Truck?.PlateNumber}",
                DriverName =
                    x.Driver?.FullName ?? "غير محدد",
                DriverPhone = x.DriverPhone,
                DepartmentName =
                    x.Department?.Name ?? "غير محدد",
                RequestStatusName =
                    x.RequestStatus?.Name ?? "غير محدد",
                EmployeeName =
                    x.CreatedBy?.Name ?? "غير محدد",
                PermitNumber = x.PermitNumber
            }))
            .ToList();

            return View(requests);
        }
        // الموردين 
        [HttpGet]
        public async Task<IActionResult> Suppliers()
        {
            var suppliers = await _supplierRepository.GetAllAsync(
                include: query => query.Include(s => s.CreatedBy)
            );

            var model = suppliers.Select(s => new SupplierListVM
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                SupCode = s.SupCode,
                LogoURL = s.LogoURL,
                HostEmployeeName = s.CreatedBy != null ? s.CreatedBy.Name : "غير محدد"
            }).ToList();

            return View(model);
        }

    }
}