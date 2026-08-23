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
        private readonly ITicketEngineService _ticketEngineService;

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
            IRepository<DockAssignment> dockAssignmentRepository,
            ITicketEngineService ticketEngineService)
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
            _ticketEngineService = ticketEngineService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int supplierid, string searchString)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierid);
            if (supplier is null)
            {
                return RedirectToAction("Index", "Supplier");
            }

            var (_, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            var trucksrepo = await _truckRepository.GetTruckWithTypeName();

            var supplierTrucks = trucksrepo
                .Where(x => x.SupplierRequests.Any(sr => sr.SupplierId == supplierid) && !activeTruckIds.Contains(x.Id))
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

            // 👇 فلترة البيانات لو المستخدم كتب حاجة في خانة البحث
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
        public async Task<IActionResult> Edit(int? id, int? supplierId)
        {
            if (id == null)
            {
                ModelState.AddModelError("الشاحنة", "رقم الشاحنة مفقود.");
                return View(new UpdateTruckVM());
            }

            var truck = await _truckRepository.GetByIdAsync(id.Value);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                return View(new UpdateTruckVM());
            }

            var supplierRequest = truck.SupplierRequests?.FirstOrDefault();
            string companyName = supplierRequest?.Supplier?.Name ?? "فتح الله";

            var vm = new UpdateTruckVM
            {
                Id = truck.Id,
                TruckNumber = truck.PlateNumber,
                TruckLetters = truck.PlateLetter,
                Capacity = truck.StorageCapacity,
                IsRefrigerated = truck.IsRefrigerated,
                CompanyName = companyName,
                SupplierId = supplierId ?? supplierRequest?.SupplierId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateTruckVM update)
        {
            if (!ModelState.IsValid)
            {
                return View(update);
            }

            var truck = await _truckRepository.GetByIdAsync(update.Id);

            if (truck == null)
            {
                ModelState.AddModelError("الشاحنة", "هذه الشاحنة غير موجودة.");
                return View(update);
            }

            var plateNum = update.TruckNumber?.Trim() ?? "";
            var plateLet = update.TruckLetters?.Trim() ?? "";

            if (await _truckRepository.ExistsAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && x.Id != update.Id && !x.IsDeleted))
            {
                ModelState.AddModelError(nameof(update.TruckNumber), "رقم وحروف اللوحة مسجلة بالفعل لشاحنة أخرى.");
                return View(update);
            }

            truck.PlateNumber = plateNum;
            truck.PlateLetter = plateLet;
            truck.StorageCapacity = update.Capacity;
            truck.IsRefrigerated = update.IsRefrigerated;

            _truckRepository.Update(truck);
            await _truckRepository.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات الشاحنة بنجاح.";

            if (update.SupplierId.HasValue && update.SupplierId.Value > 0)
            {
                return RedirectToAction(nameof(Index), new { supplierid = update.SupplierId.Value });
            }

            return RedirectToAction(nameof(MainTraDrivers));
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
            var (_, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();
            var allTrucks = await _truckRepository.GetAllAsync();

            var truckList = allTrucks
                .Where(d => !activeTruckIds.Contains(d.Id))
                .Select(d => new TruckListVM
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
            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            var allDrivers = await _driverRepository.GetAllAsync();
            var availableDrivers = allDrivers.Where(d => !activeDriverIds.Contains(d.Id)).ToList();
            var driversList = availableDrivers.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName }).ToList();

            string? supplierName = null;

            if (supplierId.HasValue && supplierId.Value > 0)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value);
                supplierName = supplier?.Name;
            }

            var model = new Rassef.ViewModels.Truck.AddTraTruckVM
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                Suppliers = suppliers,
                Drivers = driversList
            };

            ViewBag.Suppliers = suppliers;
            ViewBag.SupplierName = supplierName;

            return View(model);
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTraDriver(Rassef.ViewModels.Truck.AddTraTruckVM create)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUserId = parsedId;
            }
            var currentUser = await _userRepository.GetByIdAsync(currentUserId);

            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            var plateNum = create.PlateNumber?.Trim() ?? "";
            var plateLet = create.PlateLetter?.Trim() ?? "";

            var existingTruck = await _truckRepository.FindAsync(x => x.PlateNumber == plateNum && x.PlateLetter == plateLet && !x.IsDeleted);

            if (existingTruck != null)
            {
                if (activeTruckIds.Contains(existingTruck.Id))
                {
                    ModelState.AddModelError("PlateNumber", "الشاحنة لديها دور نشط حالياً (في الانتظار أو قيد التفريغ). يجب إنهاء الدور السابق أولاً.");
                    create.Suppliers = await GetSuppliersAsync();
                    ViewBag.Suppliers = create.Suppliers;
                    ViewBag.SupplierName = create.SupplierName;
                    return View(create);
                }
            }

            if (create.DriverId.HasValue && create.DriverId.Value > 0 && activeDriverIds.Contains(create.DriverId.Value))
            {
                ModelState.AddModelError("DriverId", "السائق المختار لديه دور نشط حالياً. يجب اكتمال الدور السابق أولاً.");
                create.Suppliers = await GetSuppliersAsync();
                ViewBag.Suppliers = create.Suppliers;
                ViewBag.SupplierName = create.SupplierName;
                return View(create);
            }

            Truck truck;
            if (existingTruck != null)
            {
                truck = existingTruck;
                truck.StorageCapacity = create.StorageCapacity ?? 0;
                truck.IsRefrigerated = create.TruckType == "تبريد";
                _truckRepository.Update(truck);
                await _truckRepository.SaveChangesAsync();
            }
            else
            {
                truck = new Truck
                {
                    PlateNumber = plateNum,
                    PlateLetter = plateLet,
                    StorageCapacity = create.StorageCapacity ?? 0,
                    IsRefrigerated = create.TruckType == "تبريد",
                    TruckTypeId = 1,
                    CreatedBy = currentUser
                };
                await _truckRepository.AddAsync(truck);
                await _truckRepository.SaveChangesAsync();
            }

            Driver? driver = null;
            if (!string.IsNullOrWhiteSpace(create.NewDriverName) && !string.IsNullOrWhiteSpace(create.NewDriverNationalId) && !string.IsNullOrWhiteSpace(create.NewDriverPhone))
            {
                driver = (await _driverRepository.GetAllAsync()).FirstOrDefault(x => x.NationalId == create.NewDriverNationalId || x.Phone == create.NewDriverPhone);
                if (driver != null && activeDriverIds.Contains(driver.Id))
                {
                    ModelState.AddModelError("NewDriverNationalId", "السائق لديه دور نشط حالياً. يجب اكتمال الدور السابق أولاً.");
                    create.Suppliers = await GetSuppliersAsync();
                    ViewBag.Suppliers = create.Suppliers;
                    ViewBag.SupplierName = create.SupplierName;
                    return View(create);
                }
                if (driver == null)
                {
                    driver = new Driver
                    {
                        FullName = create.NewDriverName,
                        NationalId = create.NewDriverNationalId,
                        Phone = create.NewDriverPhone,
                        CreatedById = currentUserId
                    };
                    await _driverRepository.AddAsync(driver);
                    await _driverRepository.SaveChangesAsync();
                }
            }

            int finalDriverId = driver?.Id ?? (create.DriverId.HasValue && create.DriverId.Value > 0 ? create.DriverId.Value : 0);
            if (finalDriverId <= 0)
            {
                var fallbackDriver = (await _driverRepository.GetAllAsync()).FirstOrDefault();
                finalDriverId = fallbackDriver?.Id ?? 1;
            }

            if (create.DepartmentId.HasValue && create.DepartmentId.Value > 0)
            {
                var allTransfers = await _transferRequestRepository.GetAllAsync();
                int nextAviz = allTransfers.Count() + 1;
                string avizNumber = $"AVIZ-{nextAviz:D4}";

                var req = new TransferRequest
                {
                    DepartmentId = create.DepartmentId.Value,
                    TruckId = truck.Id,
                    DriverId = finalDriverId,
                    PermitTypeId = 1,
                    PermitNumber = $"PER-TR-{DateTime.Now.Ticks % 100000}",
                    AvizNumber = avizNumber,
                    RequestStatusId = 1,
                    CreatedById = currentUserId.ToString(),
                    CreatedBy = currentUser!
                };
                await _transferRequestRepository.AddAsync(req);
                await _transferRequestRepository.SaveChangesAsync();

                var ticketResult = await _ticketEngineService.IssueTransferTicketAsync(create.DepartmentId.Value, req.Id, currentUserId);

                TempData["Success"] = $"تم إضافة الشاحنة وإصدار الدور رقم {ticketResult.TicketNumber} بنجاح.";
                return RedirectToAction("Recript", "Driver", new { ticketId = ticketResult.TicketId });
            }
            else
            {
                TempData["Success"] = "تم إضافة الشاحنة بنجاح.";
            }

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

            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            if (activeTruckIds.Contains(dto.TruckId))
            {
                return BadRequest(new { success = false, message = "الشاحنة لديها دور نشط حالياً (في الانتظار أو قيد التنفيذ). يجب اكتمال الدور السابق أولاً." });
            }

            if (dto.DriverId.HasValue && dto.DriverId.Value > 0 && activeDriverIds.Contains(dto.DriverId.Value))
            {
                return BadRequest(new { success = false, message = "السائق المختار لديه دور نشط حالياً. يجب اكتمال الدور السابق أولاً." });
            }

            // Get or fallback driver
            Driver? driver = null;
            if (dto.DriverId.HasValue && dto.DriverId.Value > 0)
            {
                driver = await _driverRepository.GetByIdAsync(dto.DriverId.Value);
            }
            if (driver == null)
            {
                var allDrivers = await _driverRepository.GetAllAsync();
                driver = allDrivers.FirstOrDefault(d => !activeDriverIds.Contains(d.Id));
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
                CreatedById = (currentUser?.Id ?? 1).ToString(),
                CreatedBy = currentUser!
            };

            await _transferRequestRepository.AddAsync(transferRequest);
            await _transferRequestRepository.SaveChangesAsync();

            var ticketResult = await _ticketEngineService.IssueTransferTicketAsync(department.Id, transferRequest.Id, currentUser?.Id ?? 1);

            return Json(new
            {
                success = true,
                ticketId = ticketResult.TicketId,
                ticketNumber = ticketResult.TicketNumber,
                requestType = "تحويل",
                waitingCount = ticketResult.WaitingCount,
                departmentName = ticketResult.DepartmentName,
                dockName = ticketResult.DockName,
                employeeName = ticketResult.EmployeeName,
                truckPlate = $"{truck.PlateLetter} {truck.PlateNumber}",
                driverName = driver.FullName
            });
        }

        #region Helpers
        private async Task<(HashSet<int> ActiveDriverIds, HashSet<int> ActiveTruckIds)> GetActiveDriverAndTruckIdsAsync()
        {
            var allTickets = await _ticketRepository.GetAllAsync(q => q
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest)
                .Include(t => t.TransferRequest)
            );

            var activeTickets = allTickets.Where(t =>
            {
                if (t.IsDeleted) return false;
                if (t.ExitTime != DateTimeOffset.MinValue && t.ExitTime > t.QueueTime) return false;
                if (t.TicketStatus != null)
                {
                    var n = t.TicketStatus.Name.Replace("إ", "ا").Trim().ToLower();
                    if (n.Contains("مكتمل") || n.Contains("تم") || n.Contains("خروج") || n.Contains("منتهي") || n.Contains("complete") || n.Contains("done"))
                        return false;
                }
                else if (t.TicketStatusId == 3)
                {
                    return false;
                }
                return true;
            }).ToList();

            var driverIds = activeTickets
                .Select(t => t.SupplierRequest?.DriverId ?? t.TransferRequest?.DriverId)
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id.Value)
                .ToHashSet();

            var truckIds = activeTickets
                .Select(t => t.SupplierRequest?.TruckId ?? t.TransferRequest?.TruckId)
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id.Value)
                .ToHashSet();

            return (driverIds, truckIds);
        }

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
