namespace Rassef.Controllers
{
    public class DriverController : Controller
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ITruckRepository _truckRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRepository<DriverTypes> _driverTypeRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly ISupplierRequestRepository _supplierRequestRepository;
        private readonly IRepository<PermitTypes> _permitTypeRepository;
        private readonly IRepository<CommodityTypes> _commodityTypeRepository;
        private readonly IRepository<RequestStatuses> _requestStatusRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITicketEngineService _ticketEngineService;

        public DriverController(
            IDriverRepository repository,
            ISupplierRepository supplierRepository,
            ITruckRepository truckRepository,
            IDepartmentRepository departmentRepository,
            IRepository<DriverTypes> driverTypeRepository,
            IRepository<QueueTicket> ticketRepository,
            ISupplierRequestRepository supplierRequestRepository,
            IRepository<PermitTypes> permitTypeRepository,
            IRepository<CommodityTypes> commodityTypeRepository,
            IRepository<RequestStatuses> requestStatusRepository,
            IUserRepository userRepository,
            ITicketEngineService ticketEngineService)
        {
            _driverRepository = repository;
            _supplierRepository = supplierRepository;
            _truckRepository = truckRepository;
            _departmentRepository = departmentRepository;
            _driverTypeRepository = driverTypeRepository;
            _ticketRepository = ticketRepository;
            _supplierRequestRepository = supplierRequestRepository;
            _permitTypeRepository = permitTypeRepository;
            _commodityTypeRepository = commodityTypeRepository;
            _requestStatusRepository = requestStatusRepository;
            _userRepository = userRepository;
            _ticketEngineService = ticketEngineService;
        }

        // Get All Drivers
        [HttpGet]
        public async Task<IActionResult> Index(int id, int supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);

            if (supplier is null)
                return RedirectToAction("Index", "Supplier");

            var truck = await _truckRepository.GetByIdAsync(id);

            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            // CHANGED (this pass) — added .Include(d => d.DeiverType) and a
            // DeiverType?.Code == 1 filter, per explicit instruction: this picker
            // should only offer drivers whose DriverTypes.Code is 1.
            var drivers = await _driverRepository.GetAllAsync(query =>
                query.Include(d => d.DeiverType));

            var driverList = drivers
                .Where(d => !activeDriverIds.Contains(d.Id) && d.DeiverType?.Code == 1)
                .Select(d => new DriverListVM
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    NationalId = d.NationalId,
                    Phone = d.Phone
                }).ToList();

            ViewBag.SupplierName = supplier.Name;

            ViewBag.TruckName =
                truck != null
                    ? $"{truck.PlateLetter} {truck.PlateNumber}"
                    : "سيارة غير محددة";

            ViewBag.SupplierId = supplierId;
            var depts = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();

            return View(driverList);
        }

        /// <summary>
        /// عرض وطباعة إيصال الدور (البون)
        /// Displays printable entry receipt for driver ticket
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Recript(int? ticketId)
        {
            ReceiptVM? model = null;
            QueueTicket? ticket = null;

            var allTickets = await _ticketRepository.GetAllAsync(
                query => query
                    .Include(t => t.Department)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.Supplier)
                    .Include(t => t.SupplierRequest!).ThenInclude(sr => sr!.CreatedBy)
                    .Include(t => t.TransferRequest!).ThenInclude(tr => tr!.CreatedBy)
                    .Include(t => t.DockAssignments!).ThenInclude(da => da.Dock)
            );

            if (ticketId.HasValue && ticketId.Value > 0)
            {
                ticket = allTickets.FirstOrDefault(t => t.Id == ticketId.Value);
            }
            else
            {
                ticket = allTickets.OrderByDescending(t => t.CreatedAT).FirstOrDefault();
            }

            if (ticket != null)
            {
                var isSupplier = ticket.SupplierRequestId != null || ticket.SupplierRequest != null;

                var dockName = ticket.DockAssignments?.OrderByDescending(da => da.AssignedAt).Select(da => da.Dock?.DockName).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(dockName) && ticket.Department != null)
                {
                    dockName = $"{ticket.Department.Prefix}1";
                }
                if (string.IsNullOrWhiteSpace(dockName))
                {
                    dockName = "A1";
                }

                var empName = !string.IsNullOrWhiteSpace(ticket.CreatedBy?.Name) ? ticket.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(ticket.SupplierRequest?.CreatedBy?.Name) ? ticket.SupplierRequest.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(ticket.TransferRequest?.CreatedBy?.Name) ? ticket.TransferRequest.CreatedBy.Name
                    : (!string.IsNullOrWhiteSpace(ticket.CreatedBy?.UserName) ? ticket.CreatedBy.UserName
                    : (!string.IsNullOrWhiteSpace(User.Identity?.Name) ? User.Identity.Name : "المسؤول"))));

                // حساب عدد الأدوار المنتظرة فعلياً قبل هذا الدور
                int waitingCount = allTickets.Count(t =>
                {
                    if (t.Id >= ticket.Id) return false;

                    bool matchesDept = t.DepartmentId == ticket.DepartmentId ||
                        (!string.IsNullOrWhiteSpace(t.TicketNumber) && !string.IsNullOrWhiteSpace(ticket.TicketNumber) &&
                         char.ToUpper(t.TicketNumber.Trim()[0]) == char.ToUpper(ticket.TicketNumber.Trim()[0]));

                    if (!matchesDept && ticket.DepartmentId > 0) return false;

                    if (t.ExitTime != DateTimeOffset.MinValue && t.ExitTime > t.QueueTime) return false;

                    if (t.TicketStatus != null)
                    {
                        var st = t.TicketStatus.Name.Replace("إ", "ا").Trim().ToLower();
                        if (st.Contains("مكتمل") || st.Contains("تم") || st.Contains("خروج") || st.Contains("منتهي"))
                            return false;
                    }

                    return true;
                });

                if (waitingCount == 0 && ticket.Id > 1)
                {
                    waitingCount = allTickets.Count(t =>
                        t.Id < ticket.Id &&
                        (t.ExitTime == DateTimeOffset.MinValue || t.ExitTime <= t.QueueTime) &&
                        (t.TicketStatus == null || (!t.TicketStatus.Name.Contains("مكتمل") && !t.TicketStatus.Name.Contains("تم") && !t.TicketStatus.Name.Contains("خروج")))
                    );
                }

                model = new ReceiptVM
                {
                    TicketNumber = !string.IsNullOrWhiteSpace(ticket.TicketNumber) ? ticket.TicketNumber : "A1",
                    RequestType = isSupplier ? "توريد" : "تحويل",
                    DepartmentName = ticket.Department?.Name ?? "غير محدد",
                    DockName = dockName,
                    EmployeeName = empName,
                    WaitingCount = waitingCount.ToString(),
                    CreatedAt = ticket.CreatedAT != default ? ticket.CreatedAT : DateTimeOffset.Now
                };
            }

            return View(model);
        }

        /// <summary>
        /// عرض تفاصيل السائق (GET)
        /// Displays driver details by Id
        /// </summary>
        [HttpGet]
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
        public async Task<IActionResult> Create(int? supplierId)
        {
            var model = new CreateDriverVM
            {
                SupplierId = supplierId,
                Suppliers = await GetSuppliersAsync()
            };

            if (supplierId.HasValue && supplierId.Value > 0)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value);
                if (supplier != null)
                {
                    model.SupplierName = supplier.Name;
                }
            }

            var departments = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = departments.Select(d => new { id = d.Id, name = d.Name }).ToList();

            return View(model);
        }

        private async Task PrepareCreateDriverVMAsync(CreateDriverVM create)
        {
            create.Suppliers = await GetSuppliersAsync();
            if (create.SupplierId.HasValue && create.SupplierId.Value > 0)
            {
                var supplier = await _supplierRepository.GetByIdAsync(create.SupplierId.Value);
                if (supplier != null)
                {
                    create.SupplierName = supplier.Name;
                }
            }
            var depts = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverVM create)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateDriverVMAsync(create);
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.NationalId == create.NationalId))
            {
                ModelState.AddModelError(nameof(create.NationalId), "الرقم القومي مسجل بالفعل.");
                await PrepareCreateDriverVMAsync(create);
                return View(create);
            }

            if (await _driverRepository.ExistsAsync(x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(nameof(create.Phone), "رقم الهاتف مسجل بالفعل.");
                await PrepareCreateDriverVMAsync(create);
                return View(create);
            }

            var allDriverTypes = await _driverTypeRepository.GetAllAsync();
            var defaultDriverType = allDriverTypes.FirstOrDefault();
            int driverTypeId;

            if (defaultDriverType == null)
            {
                defaultDriverType = new DriverTypes { Code = 1, Name = "عام" };
                await _driverTypeRepository.AddAsync(defaultDriverType);
                await _driverTypeRepository.SaveChangesAsync();
                driverTypeId = defaultDriverType.Id;
            }
            else
            {
                driverTypeId = defaultDriverType.Id;
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
                DeiverTypeId = driverTypeId,
                CreatedById = currentUserId
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            if (create.SupplierId.HasValue && create.SupplierId.Value > 0)
            {
                return RedirectToAction("CreateTruckWithDriver", "SupplierRequest", new { supplierId = create.SupplierId.Value, selectedDriverId = driver.Id, autoOpenModal = true });
            }

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
        [HttpGet]
        public async Task<IActionResult> TransferDrivers()
        {

            var allDrivers = await _driverRepository.GetAllAsync();

            var driverList = allDrivers.Select(d => new DriverListVM
            {
                Id = d.Id,
                FullName = d.FullName,
                NationalId = d.NationalId,
                Phone = d.Phone
            }).ToList();

            return View(driverList);
        }


        [HttpGet]
        public async Task<IActionResult> addDriverTransfer(int? supplierId, int? truckId)
        {
            string supplierName = string.Empty;

            if (supplierId.HasValue)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value);
                if (supplier != null)
                {
                    supplierName = supplier.Name;
                }
            }

            var model = new CreateDriverVM
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                TruckId = truckId,
                Suppliers = await GetSuppliersAsync()
            };

            return View(model);
        }

        // Create Transfer (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addDriverTransfer(CreateDriverVM create)
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

            var allDriverTypes = await _driverTypeRepository.GetAllAsync();
            var defaultDriverType = allDriverTypes.FirstOrDefault();
            int driverTypeId;

            if (defaultDriverType == null)
            {
                defaultDriverType = new DriverTypes { Code = 1, Name = "عام" };
                await _driverTypeRepository.AddAsync(defaultDriverType);
                await _driverTypeRepository.SaveChangesAsync();
                driverTypeId = defaultDriverType.Id;
            }
            else
            {
                driverTypeId = defaultDriverType.Id;
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
                DeiverTypeId = driverTypeId,
                CreatedById = currentUserId
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            // يمكنك التوجيه لصفحة الشاحنات مع إرجاع الـ truckId و supplierId إذا أردت متابعة الشاحنة
            return RedirectToAction(nameof(Index), new { supplierId = create.SupplierId, id = create.TruckId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplierTicket([FromBody] CreateSupplierTicketDto dto)
        {
            if (dto == null || dto.SupplierId <= 0 || dto.TruckId <= 0 || dto.DriverId <= 0 || dto.DepartmentId <= 0)
            {
                return BadRequest(new { success = false, message = "بيانات غير مكتملة. يرجى اختيار المورد والسيارة والسائق والقسم." });
            }

            var supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId);
            if (supplier == null)
            {
                return NotFound(new { success = false, message = "المورد غير موجود." });
            }

            var truck = await _truckRepository.GetByIdAsync(dto.TruckId);
            if (truck == null)
            {
                return NotFound(new { success = false, message = "الشاحنة غير موجودة." });
            }

            var driver = await _driverRepository.GetByIdAsync(dto.DriverId);
            if (driver == null)
            {
                return NotFound(new { success = false, message = "السائق غير موجود." });
            }

            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (department == null)
            {
                return NotFound(new { success = false, message = "القسم غير موجود." });
            }

            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            if (activeTruckIds.Contains(truck.Id))
            {
                return BadRequest(new { success = false, message = "الشاحنة لديها دور نشط حالياً (في الانتظار أو قيد التفريغ). يجب اكتمال الدور السابق أولاً." });
            }

            if (activeDriverIds.Contains(driver.Id))
            {
                return BadRequest(new { success = false, message = "السائق لديه دور نشط حالياً (في الانتظار أو قيد التفريغ). يجب اكتمال الدور السابق أولاً." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return BadRequest(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return BadRequest(new { success = false, message = "لم يتم العثور على المستخدم." });
            }

            var defaultPermit = (await _permitTypeRepository.GetAllAsync()).FirstOrDefault();
            var defaultCommodity = (await _commodityTypeRepository.GetAllAsync()).FirstOrDefault();
            var defaultStatus = (await _requestStatusRepository.GetAllAsync()).FirstOrDefault();

            if (defaultPermit == null || defaultCommodity == null || defaultStatus == null)
            {
                return BadRequest(new { success = false, message = "بيانات إعداد النظام غير مكتملة (نوع التصريح / نوع البضاعة / حالة الطلب)." });
            }

            var supplierRequest = new SupplierRequest
            {
                SupplierId = supplier.Id,
                TruckId = truck.Id,
                DriverId = driver.Id,
                DepartmentId = department.Id,
                PermitTypeId = defaultPermit.Id,
                CommodityTypeId = defaultCommodity.Id,
                RequestStatusId = defaultStatus.Id,
                CreatedBy = currentUser
            };

            await _supplierRequestRepository.AddAsync(supplierRequest);
            await _supplierRequestRepository.SaveChangesAsync();

            var ticketResult = await _ticketEngineService.IssueSupplierTicketAsync(department.Id, supplierRequest.Id, currentUser.Id);

            return Json(new
            {
                success = true,
                ticketId = ticketResult.TicketId,
                ticketNumber = ticketResult.TicketNumber,
                requestType = "توريد",
                waitingCount = ticketResult.WaitingCount,
                departmentName = ticketResult.DepartmentName,
                dockName = ticketResult.DockName,
                employeeName = ticketResult.EmployeeName,
                companyName = supplier.Name,
                truckPlate = $"{truck.PlateLetter} {truck.PlateNumber}",
                driverName = driver.FullName
            });
        }

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
    }
}