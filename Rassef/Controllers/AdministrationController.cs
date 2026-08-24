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
        private readonly ITicketEngineService _ticketEngineService;

        public AdministrationController(
            IUserRepository userRepository,
            IRepository<Position> positionRepository,
            IRepository<Truck> truckRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverTypes> driverTypeRepository,
            ITransferRequestRepository transferRequestRepository,
            ISupplierRequestRepository supplierRequestRepository,
            IRepository<Supplier> supplierRepository,
            ITicketEngineService ticketEngineService)
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
            _ticketEngineService = ticketEngineService;
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
            var positions = await _positionRepository.GetAllAsync(q => q.Where(p => !p.IsDeleted));

            ViewBag.Positions = positions;

            return View(new EmployeeCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateVM model)
        {
            // فحص فرادة الرقم القومي للموظف
            var existingUser = await _userRepository.FindAsync(u => u.NationalId == model.NationalId && !u.IsDeleted);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.NationalId), "الرقم القومي مُسجل لموظف آخر بالفعل.");
            }

            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync(q => q.Where(p => !p.IsDeleted));
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

            var positions = await _positionRepository.GetAllAsync(q => q.Where(p => !p.IsDeleted));

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
            // فحص فرادة الرقم القومي عند التعديل
            var existingUser = await _userRepository.FindAsync(u => u.NationalId == model.NationalId && u.Id != model.Id && !u.IsDeleted);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.NationalId), "الرقم القومي مُسجل لموظف آخر بالفعل.");
            }

            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync(q => q.Where(p => !p.IsDeleted));
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
                     .Include(t => t.SupplierRequests)
                        .ThenInclude(sr => sr.Supplier)
                     .Include(t => t.TransferRequests)
            );

            var truckList = trucks.Select(t =>
            {
                string companyName = "غير محدد";
                var latestSupplier = t.SupplierRequests?.OrderByDescending(r => r.CreatedAT).FirstOrDefault();
                var latestTransfer = t.TransferRequests?.OrderByDescending(r => r.CreatedAT).FirstOrDefault();

                if (latestSupplier != null && latestTransfer != null)
                {
                    if (latestSupplier.CreatedAT > latestTransfer.CreatedAT)
                    {
                        companyName = latestSupplier.Supplier?.Name ?? "غير محدد";
                    }
                    else
                    {
                        companyName = "تحويل داخلي";
                    }
                }
                else if (latestSupplier != null)
                {
                    companyName = latestSupplier.Supplier?.Name ?? "غير محدد";
                }
                else if (latestTransfer != null)
                {
                    companyName = "تحويل داخلي";
                }
                else if (t.TruckType != null)
                {
                    companyName = t.TruckType.Name;
                }

                return new ViewModels.Administration.TruckListVM
                {
                    Id = t.Id,
                    PlateNumber = $"{t.PlateLetter} {t.PlateNumber}",
                    IsRefrigerated = t.IsRefrigerated ? "تبريد" : "لا تبريد",
                    Company = companyName,
                    StorageCapacity = t.StorageCapacity,
                    HostEmployeeName = !string.IsNullOrWhiteSpace(t.CreatedBy?.Name) ? t.CreatedBy.Name : (!string.IsNullOrWhiteSpace(t.CreatedBy?.UserName) ? t.CreatedBy.UserName : "المسؤول")
                };
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

            // التحقق من عدم تكرار رقم وحروف اللوحة معاً
            var plateNum = model.PlateNumber?.Trim() ?? "";
            var plateLet = model.PlateLetter?.Trim() ?? "";

            if (await _truckRepository.ExistsAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(model.PlateNumber), "رقم وحروف اللوحة مسجلة بالفعل لشاحنة أخرى.");
                ViewBag.TruckTypes = await _truckTypesRepository.GetAllAsync();
                return View(model);
            }

            // جلب معرف الموظف الحالي من الـ Claims بأمان
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUserId = parsedId;
            }

            var truck = new Truck
            {
                PlateLetter = plateLet,
                PlateNumber = plateNum,
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

            var plateNum = model.PlateNumber?.Trim() ?? "";
            var plateLet = model.PlateLetter?.Trim() ?? "";

            if (await _truckRepository.ExistsAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && x.Id != model.Id && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(model.PlateNumber), "رقم وحروف اللوحة مسجلة بالفعل لشاحنة أخرى.");
                ViewBag.TruckTypes = await _truckTypesRepository.GetAllAsync();
                return View(model);
            }

            truck.PlateLetter = plateLet;
            truck.PlateNumber = plateNum;
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
                    .Include(d => d.SupplierRequests!)
                        .ThenInclude(sr => sr.Supplier)
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

            var firstSupplierReq = driver.SupplierRequests?.FirstOrDefault();
            string companyName = firstSupplierReq?.Supplier?.Name ?? (driver.DeiverType?.Name ?? "فتح الله");

            var driverDetails = new ViewModels.Administration.DriverDetailsVM
            {
                Id = driver.Id,
                FullName = driver.FullName,
                DriverType = driver.DeiverType?.Name ?? "غير محدد",
                Company = companyName,
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
            var existingDriver = await _driverRepository.FindAsync(d => d.NationalId == model.NationalId && !d.IsDeleted);
            if (existingDriver != null)
            {
                ModelState.AddModelError(nameof(model.NationalId), "الرقم القومي مُسجل لسائق آخر بالفعل.");
            }

            if (!ModelState.IsValid)
            {
                var driverTypes = await _driverTypeRepository.GetAllAsync();

                ViewBag.DriverTypes = driverTypes;

                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUserId = parsedId;
            }

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
            var existingDriver = await _driverRepository.FindAsync(d => d.NationalId == model.NationalId && d.Id != model.Id && !d.IsDeleted);
            if (existingDriver != null)
            {
                ModelState.AddModelError(nameof(model.NationalId), "الرقم القومي مُسجل لسائق آخر بالفعل.");
            }

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
            var requestsRepo = await _Transferrepository.GetAllWithDetailsAsync();

            var requests = requestsRepo.Select(x =>
            {
                var ticket = x.QueueTickets?.OrderByDescending(q => q.CreatedAT).FirstOrDefault();
                var dockName = ticket?.DockAssignments?
                    .OrderByDescending(da => da.AssignedAt)
                    .Select(da => da.Dock?.DockName)
                    .FirstOrDefault() ?? "A1";

                var empName = !string.IsNullOrWhiteSpace(x.CreatedBy?.Name) ? x.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(ticket?.CreatedBy?.Name) ? ticket.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(x.CreatedBy?.UserName) ? x.CreatedBy.UserName
                    : (!string.IsNullOrWhiteSpace(ticket?.CreatedBy?.UserName) ? ticket.CreatedBy.UserName
                    : "المسؤول")));

                return new TransferRequestListVM
                {
                    Id = x.Id,
                    RequestType = "تحويل",
                    TicketNumber = ticket?.TicketNumber ?? "TR-0001",
                    AvizNumber = !string.IsNullOrWhiteSpace(x.AvizNumber) ? x.AvizNumber : $"AVIZ-{x.Id:D4}",
                    DateTime = ticket?.QueueTime ?? x.CreatedAT,
                    DepartmentName = x.Department?.Name ?? "غير محدد",
                    DriverName = x.Driver?.FullName ?? "غير محدد",
                    EmployeeName = empName,
                    TruckPlateNumber = x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}" : "غير محدد",
                    RequestStatus = x.RequestStatus?.Name ?? "قيد الانتظار",
                    DockName = dockName
                };
            }).ToList();

            return View(requests);
        }

        [HttpGet]
        // طلبات التوريد - Index
        public async Task<IActionResult> SupplierRequests()
        {
            var requestsRepo = await _supplierRequestRepository.GetAllWithDetailsAsync();

            var requests = requestsRepo.Select(x =>
            {
                var ticket = x.QueueTickets?.OrderByDescending(q => q.CreatedAT).FirstOrDefault();
                var dockName = ticket?.DockAssignments?
                    .OrderByDescending(da => da.AssignedAt)
                    .Select(da => da.Dock?.DockName)
                    .FirstOrDefault() ?? "A1";

                var empName = !string.IsNullOrWhiteSpace(x.CreatedBy?.Name) ? x.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(ticket?.CreatedBy?.Name) ? ticket.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(x.CreatedBy?.UserName) ? x.CreatedBy.UserName
                    : (!string.IsNullOrWhiteSpace(ticket?.CreatedBy?.UserName) ? ticket.CreatedBy.UserName
                    : "المسؤول")));

                return new SupplierRequestListVM
                {
                    Id = x.Id,
                    RequestType = "توريد",
                    TicketId = ticket?.Id,
                    TicketNumber = ticket?.TicketNumber ?? "A1",
                    TicketStatusName = ticket?.TicketStatus?.Name ?? "إنتظار",
                    QueueTime = ticket?.QueueTime ?? x.CreatedAT,
                    DockName = dockName,
                    SupplierName = x.Supplier?.Name ?? "غير محدد",
                    TruckPlateNumber = x.Truck != null ? $"{x.Truck.PlateLetter} {x.Truck.PlateNumber}" : "غير محدد",
                    DriverName = x.Driver?.FullName ?? "غير محدد",
                    DriverPhone = x.DriverPhone ?? x.Driver?.Phone ?? "",
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name ?? "غير محدد",
                    RequestStatusName = x.RequestStatus?.Name ?? "قيد الانتظار",
                    EmployeeName = empName,
                    PermitNumber = x.PermitNumber ?? ""
                };
            }).ToList();

            return View(requests);
        }

        // استدعاء الدور القادم (Next)
        [HttpPost]
        public async Task<IActionResult> CallNextSupplierRequest(int? departmentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.CallNextTicketAsync(departmentId, currentUserId);
            return Json(result);
        }

        // تحديث حالة تذكرة / دور معين (انتظار / جاري / مكتمل)
        [HttpPost]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusDTO dto)
        {
            if (dto == null || dto.TicketId <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            {
                return Json(new { success = false, message = "بيانات غير صالحة." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.UpdateTicketStatusAsync(dto.TicketId, dto.Status, currentUserId);
            return Json(result);
        }

        // إنهاء الدور (Completed)
        [HttpPost]
        public async Task<IActionResult> CompleteTicket([FromBody] UpdateTicketStatusDTO dto)
        {
            if (dto == null || dto.TicketId <= 0)
            {
                return Json(new { success = false, message = "رقم التذكرة غير صالح." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.UpdateTicketStatusAsync(dto.TicketId, "مكتمل", currentUserId);
            return Json(result);
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

        [HttpGet]
        public async Task<IActionResult> SupplierDetails(int id)
        {
            var suppliers = await _supplierRepository.GetAllAsync(query =>
                query.Where(s => !s.IsDeleted)
                     .Include(s => s.CreatedBy)
                     .Include(s => s.SupplierRequests)
            );

            var supplier = suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier == null)
            {
                ModelState.AddModelError(string.Empty, "هذا المورد غير موجود.");
                return RedirectToAction(nameof(Suppliers));
            }

            int visitsCount = supplier.SupplierRequests?.Count ?? 0;

            var model = new ViewModels.Administration.SupplierDetailsVM
            {
                Id = supplier.Id,
                Name = supplier.Name,
                SupCode = !string.IsNullOrWhiteSpace(supplier.SupCode) ? supplier.SupCode : $"j0{supplier.Id:D6}",
                Phone = !string.IsNullOrWhiteSpace(supplier.Phone) ? supplier.Phone : "01002670738",
                LogoURL = supplier.LogoURL,
                HostEmployeeName = supplier.CreatedBy?.Name ?? "محمد السيد بدير الشناوي",
                VisitsCount = visitsCount > 0 ? visitsCount : 30
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplierDeleteConfirmed(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier != null)
            {
                supplier.IsDeleted = true;
                supplier.MarkAsUpdated();
                _supplierRepository.Update(supplier);
                await _supplierRepository.SaveChangesAsync();
                TempData["Success"] = "تم حذف المورد بنجاح.";
            }
            return RedirectToAction(nameof(Suppliers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplierRequestDeleteConfirmed(int id)
        {
            var req = await _supplierRequestRepository.GetByIdAsync(id);
            if (req != null)
            {
                req.IsDeleted = true;
                req.MarkAsUpdated();
                _supplierRequestRepository.Update(req);
                await _supplierRequestRepository.SaveChangesAsync();
                TempData["Success"] = "تم حذف طلب التوريد بنجاح.";
            }
            return RedirectToAction(nameof(SupplierRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferRequestDeleteConfirmed(int id)
        {
            var req = await _Transferrepository.GetByIdAsync(id);
            if (req != null)
            {
                req.IsDeleted = true;
                req.MarkAsUpdated();
                _Transferrepository.Update(req);
                await _Transferrepository.SaveChangesAsync();
                TempData["Success"] = "تم حذف طلب التحويل بنجاح.";
            }
            return RedirectToAction(nameof(TransferRequests));
        }
    }
}