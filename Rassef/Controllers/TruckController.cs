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
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITransferRequestRepository _transferRequestRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<RequestStatuses> _requestStatusRepository;
        private readonly IRepository<PermitTypes> _permitTypeRepository;
        private readonly IRepository<Dock> _dockRepository;
        private readonly IRepository<DockAssignment> _dockAssignmentRepository;

        public TruckController(
            ITruckRepository truckRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<User> userRepository,
            ISupplierRepository supplierRepository,
            IDriverRepository driverRepository,
            IRepository<TruckTypes> truckTypes,
            IDepartmentRepository departmentRepository,
            ITransferRequestRepository transferRequestRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IRepository<RequestStatuses> requestStatusRepository,
            IRepository<PermitTypes> permitTypeRepository,
            IRepository<Dock> dockRepository,
            IRepository<DockAssignment> dockAssignmentRepository)
        {
            _truckRepository = truckRepository;
            _truckTypeRepository = truckTypeRepository;
            _userRepository = userRepository;
            _supplierRepository = supplierRepository;
            _driverRepository = driverRepository;
            _truckTypes = truckTypes;
            _departmentRepository = departmentRepository;
            _transferRequestRepository = transferRequestRepository;
            _ticketRepository = ticketRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _requestStatusRepository = requestStatusRepository;
            _permitTypeRepository = permitTypeRepository;
            _dockRepository = dockRepository;
            _dockAssignmentRepository = dockAssignmentRepository;
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

            var plateNum = create.PlateNumber?.Trim() ?? "";
            var plateLet = create.PlateLetter?.Trim() ?? "";

            if (await _truckRepository.ExistsAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(create.PlateNumber), "رقم وحروف اللوحة مسجلة بالفعل لشاحنة أخرى.");
                await LoadTruckTypesAsync(create.TruckTypeId);
                return View(create);
            }

            var truck = new Truck
            {
                PlateNumber = plateNum,
                PlateLetter = plateLet,
                StorageCapacity = create.StorageCapacity,
                IsRefrigerated = create.IsRefrigerated,
                TruckTypeId = create.TruckTypeId,
                CreatedBy = currentUser
            };

            await _truckRepository.AddAsync(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الشاحنة بنجاح.";
            return RedirectToAction(nameof(Index), new { supplierid = create.SupplierId });
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

            var plateNum = update.PlateNumber?.Trim() ?? "";
            var plateLet = update.PlateLetter?.Trim() ?? "";

            if (await _truckRepository.ExistsAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && x.Id != update.Id && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(update.PlateNumber), "رقم وحروف اللوحة مسجلة بالفعل لشاحنة أخرى.");
                await LoadTruckTypesAsync(update.TruckTypeId);
                return View(update);
            }

            truck.PlateNumber = plateNum;
            truck.PlateLetter = plateLet;
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

            var depts = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();

            return View(truckList);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> AddTraDriver(int? supplierId)
        {
            var suppliers = await GetSuppliersAsync();
            string? supplierName = null;

            if (supplierId.HasValue && supplierId.Value > 0)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value);
                supplierName = supplier?.Name;
            }

            var model = new CreateDriverVM
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                Suppliers = suppliers
            };

            ViewBag.Suppliers = suppliers;
            ViewBag.SupplierName = supplierName;

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
                if (create.SupplierId.HasValue && create.SupplierId.Value > 0)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(create.SupplierId.Value);
                    create.SupplierName = supplier?.Name;
                }
                ViewBag.Suppliers = create.Suppliers;
                ViewBag.SupplierName = create.SupplierName;
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.NationalId == create.NationalId))
            {
                ModelState.AddModelError(nameof(create.NationalId), "الرقم القومي مسجل بالفعل.");
                create.Suppliers = await GetSuppliersAsync();
                if (create.SupplierId.HasValue && create.SupplierId.Value > 0)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(create.SupplierId.Value);
                    create.SupplierName = supplier?.Name;
                }
                ViewBag.Suppliers = create.Suppliers;
                ViewBag.SupplierName = create.SupplierName;
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(nameof(create.Phone), "رقم الهاتف مسجل بالفعل.");
                create.Suppliers = await GetSuppliersAsync();
                if (create.SupplierId.HasValue && create.SupplierId.Value > 0)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(create.SupplierId.Value);
                    create.SupplierName = supplier?.Name;
                }
                ViewBag.Suppliers = create.Suppliers;
                ViewBag.SupplierName = create.SupplierName;
                return View(create);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUserId = parsedId;
            }

            var driver = new Driver
            {
                FullName = create.FullName,
                NationalId = create.NationalId,
                Phone = create.Phone,
                CreatedById = currentUserId
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            return RedirectToAction(nameof(MainTraDrivers));
        }
        [HttpPost]
        public async Task<IActionResult> CreateTransferTicket([FromBody] CreateTransferTicketDto dto)
        {
            if (dto == null || dto.TruckId <= 0 || dto.DepartmentId <= 0)
            {
                return BadRequest(new { success = false, message = "بيانات غير مكتملة." });
            }

            var truck = await _truckRepository.GetByIdAsync(dto.TruckId);
            if (truck == null)
            {
                return NotFound(new { success = false, message = "الشاحنة غير موجودة." });
            }

            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (department == null)
            {
                return NotFound(new { success = false, message = "القسم غير موجود." });
            }

            // Get or fallback driver
            Driver? driver = null;
            if (dto.DriverId.HasValue && dto.DriverId.Value > 0)
            {
                driver = await _driverRepository.GetByIdAsync(dto.DriverId.Value);
            }
            if (driver == null)
            {
                driver = (await _driverRepository.GetAllAsync()).FirstOrDefault();
            }
            if (driver == null)
            {
                driver = new Driver
                {
                    FullName = "سائق تحويل عام",
                    NationalId = $"NAT{DateTime.Now.Ticks % 100000000}",
                    Phone = "01000000000"
                };
                await _driverRepository.AddAsync(driver);
                await _driverRepository.SaveChangesAsync();
            }

            // Get current logged-in user
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            User? currentUser = null;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUser = await _userRepository.GetByIdAsync(parsedId);
            }
            if (currentUser == null)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var allUsers = await _userRepository.GetAllAsync();
                    currentUser = allUsers.FirstOrDefault(u => u.Email != null && u.Email.ToString() == email);
                }
            }
            if (currentUser == null)
            {
                currentUser = (await _userRepository.GetAllAsync()).FirstOrDefault();
            }

            var defaultPermit = (await _permitTypeRepository.GetAllAsync()).FirstOrDefault();
            var defaultStatus = (await _requestStatusRepository.GetAllAsync()).FirstOrDefault();

            var allTransfers = await _transferRequestRepository.GetAllAsync();
            int nextAvizNumber = allTransfers.Count() + 1;
            string avizNumber = $"AVIZ-{nextAvizNumber:D4}";

            var transferRequest = new TransferRequest
            {
                TruckId = truck.Id,
                DriverId = driver.Id,
                DepartmentId = department.Id,
                PermitTypeId = defaultPermit?.Id ?? 1,
                PermitNumber = $"PER-TR-{DateTime.Now.Ticks % 100000}",
                AvizNumber = avizNumber,
                RequestStatusId = defaultStatus?.Id ?? 1,
                CreatedById = currentUser?.Id.ToString() ?? "1",
                CreatedBy = currentUser!
            };

            await _transferRequestRepository.AddAsync(transferRequest);
            await _transferRequestRepository.SaveChangesAsync();

            // Generate QueueTicket
            string prefix = !string.IsNullOrWhiteSpace(department.Prefix) ? department.Prefix.Trim() : "TR";
            if (string.IsNullOrWhiteSpace(prefix)) prefix = "A";

            var allTickets = await _ticketRepository.GetAllAsync();
            var lastTicket = allTickets
                .Where(x => x.DepartmentId == department.Id)
                .OrderByDescending(x => x.CreatedAT)
                .FirstOrDefault();

            int counter = 1;
            if (lastTicket != null && !string.IsNullOrWhiteSpace(lastTicket.TicketNumber))
            {
                var digits = new string(lastTicket.TicketNumber.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrWhiteSpace(digits) && int.TryParse(digits, out var parsedCounter))
                    counter = parsedCounter + 1;
            }

            string ticketNumber = $"{prefix}{counter}";

            var allTicketStatuses = await _ticketStatusRepository.GetAllAsync();
            var ticketStatus = allTicketStatuses.FirstOrDefault(s => s.Name.Contains("انتظار") || s.Name.Contains("إنتظار")) ?? allTicketStatuses.FirstOrDefault();

            var queueTicket = new QueueTicket
            {
                TicketNumber = ticketNumber,
                DepartmentId = department.Id,
                TicketStatusId = ticketStatus?.Id ?? 1,
                TransferRequestId = transferRequest.Id,
                QueueTime = DateTimeOffset.Now,
                EntryTime = DateTimeOffset.Now,
                ExitTime = DateTimeOffset.MinValue,
                CreatedBy = currentUser
            };

            await _ticketRepository.AddAsync(queueTicket);
            await _ticketRepository.SaveChangesAsync();

            // Assign to dock if available
            var allDocks = await _dockRepository.GetAllAsync();
            var availableDock = allDocks.FirstOrDefault(d => d.DepartmentId == department.Id);
            string dockName = availableDock?.DockName ?? $"{prefix}1";

            if (availableDock != null)
            {
                var dockAssignment = new DockAssignment
                {
                    DockId = availableDock.Id,
                    TicketId = queueTicket.Id,
                    AssignedAt = DateTimeOffset.Now,
                    CreatedBy = currentUser!
                };
                await _dockAssignmentRepository.AddAsync(dockAssignment);
                await _dockAssignmentRepository.SaveChangesAsync();
            }

            string employeeName = currentUser?.Name ?? (!string.IsNullOrWhiteSpace(currentUser?.UserName) ? currentUser.UserName : (User.Identity?.Name ?? "المسؤول"));

            return Json(new
            {
                success = true,
                ticketId = queueTicket.Id,
                ticketNumber = ticketNumber,
                requestType = "تحويل",
                waitingCount = allTickets.Count(t => t.DepartmentId == department.Id && t.TicketStatusId == (ticketStatus?.Id ?? 1)),
                departmentName = department.Name,
                dockName = dockName,
                employeeName = employeeName,
                truckPlate = $"{truck.PlateLetter} {truck.PlateNumber}",
                driverName = driver.FullName
            });
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

    public class CreateTransferTicketDto
    {
        public int TruckId { get; set; }
        public int DepartmentId { get; set; }
        public int? DriverId { get; set; }
    }
}
