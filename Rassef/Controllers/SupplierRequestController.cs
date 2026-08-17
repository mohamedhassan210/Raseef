namespace Rassef.Controllers
{
    public class SupplierRequestController : Controller
    {
        private readonly ISupplierRequestRepository _supplierRequestRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<PermitTypes> _permitTypeRepository;
        private readonly IRepository<CommodityTypes> _commodityTypeRepository;
        private readonly IRepository<RequestStatuses> _requestStatusRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<TruckTypes> _truckTypeRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<QueueSettings> _queueSettingsRepository;
        private readonly IRepository<Shift> _shiftRepository;

        public SupplierRequestController(
            ISupplierRequestRepository supplierRequestRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Department> departmentRepository,
            IRepository<PermitTypes> permitTypeRepository,
            IRepository<CommodityTypes> commodityTypeRepository,
            IRepository<RequestStatuses> requestStatusRepository,
            IRepository<User> userRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IRepository<QueueSettings> queueSettingsRepository,
            IRepository<Shift> shiftRepository)
        {
            _supplierRequestRepository = supplierRequestRepository;
            _supplierRepository = supplierRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _departmentRepository = departmentRepository;
            _permitTypeRepository = permitTypeRepository;
            _commodityTypeRepository = commodityTypeRepository;
            _requestStatusRepository = requestStatusRepository;
            _userRepository = userRepository;
            _truckTypeRepository = truckTypeRepository;
            _ticketRepository = ticketRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _shiftRepository = shiftRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = await _supplierRequestRepository.GetAllAsync(
                include: query => query
                    .Include(r => r.Supplier)
                    .Include(r => r.Truck)
                    .Include(r => r.Driver)
                    .Include(r => r.Department)
                    .Include(r => r.PermitType)
                    .Include(r => r.CommodityType)
                    .Include(r => r.RequestStatus)
                    .Include(r => r.CreatedBy)
            );

            var model = requests.Select(request => new SupplierRequestDetailsVM
            {
                Id = request.Id,
                SupplierName = request.Supplier?.Name ?? "غير محدد",
                TruckInfo = request.Truck != null ? $"{request.Truck.PlateLetter} {request.Truck.PlateNumber}" : "غير محدد",
                DriverName = request.Driver?.FullName ?? "غير محدد",
                DriverPhone = request.DriverPhone,
                DriverNationalCardPhoto = request.DriverNationalCardPhoto,
                DepartmentName = request.Department?.Name ?? "غير محدد",
                PermitTypeName = request.PermitType?.Name ?? "غير محدد",
                PermitNumber = request.PermitNumber,
                CommodityTypeName = request.CommodityType?.Name ?? "غير محدد",
                RequestStatusName = request.RequestStatus?.Name ?? "غير محدد",
                IsFood = request.IsFood,
                CreatedByName = request.CreatedBy?.Name ?? "النظام"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        // Display details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("طلب المورد", "رقم الطلب مفقود.");
                return View(new SupplierRequestDetailsVM());
            }

            var request = await _supplierRequestRepository.GetByIdWithDetailsAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError("طلب المورد", "هذا الطلب غير موجود.");
                return View(new SupplierRequestDetailsVM());
            }

            var requestDetails = new SupplierRequestDetailsVM
            {
                Id = request.Id,
                SupplierName = request.Supplier?.Name ?? "غير محدد",
                TruckInfo = $"{request.Truck?.PlateLetter} {request.Truck?.PlateNumber}",
                DriverName = request.Driver?.FullName ?? "غير محدد",
                DriverPhone = request.DriverPhone,
                DriverNationalCardPhoto = request.DriverNationalCardPhoto,
                DepartmentName = request.Department?.Name ?? "غير محدد",
                PermitTypeName = request.PermitType?.Name ?? "غير محدد",
                PermitNumber = request.PermitNumber,
                CommodityTypeName = request.CommodityType?.Name ?? "غير محدد",
                RequestStatusName = request.RequestStatus?.Name ?? "غير محدد",
                IsFood = request.IsFood,
                CreatedByName = request.CreatedBy?.Name ?? "النظام"
            };

            return View(requestDetails);
        }

        [HttpGet]
        // Display create page
        public async Task<IActionResult> Create()
        {
            var vm = new CreateSupplierRequestVM();
            return View(await PopulateDropdownsAsync(vm));
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        // Create new item
        public async Task<IActionResult> CreateConfirmed(CreateSupplierRequestVM create)
        {
            if (!ModelState.IsValid)
            {
                return View(await PopulateDropdownsAsync(create));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError("", "يجب تسجيل الدخول أولاً.");
                return View(await PopulateDropdownsAsync(create));
            }

            var currentUser = await _userRepository.GetByIdAsync(int.Parse(userId));

            if (currentUser is null)
            {
                ModelState.AddModelError("", "لم يتم العثور على المستخدم.");
                return View(await PopulateDropdownsAsync(create));
            }

            var request = new SupplierRequest
            {
                SupplierId = create.SupplierId,
                TruckId = create.TruckId,
                DriverId = create.DriverId,
                DepartmentId = create.DepartmentId,
                PermitTypeId = create.PermitTypeId,
                CommodityTypeId = create.CommodityTypeId,
                RequestStatusId = create.RequestStatusId,
                DriverNationalCardPhoto = create.DriverNationalCardPhoto,
                DriverPhone = create.DriverPhone,
                PermitNumber = create.PermitNumber,
                IsFood = create.IsFood,
                CreatedBy = currentUser
            };

            await _supplierRequestRepository.AddAsync(request);
            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة طلب المورد بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display update page
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("طلب المورد", "رقم الطلب مفقود.");
                return View(await PopulateDropdownsAsync(new UpdateSupplierRequestVM()));
            }

            var request = await _supplierRequestRepository.GetByIdAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError("طلب المورد", "هذا الطلب غير موجود.");
                return View(await PopulateDropdownsAsync(new UpdateSupplierRequestVM()));
            }

            var vm = new UpdateSupplierRequestVM
            {
                Id = request.Id,
                SupplierId = request.SupplierId,
                TruckId = request.TruckId,
                DriverId = request.DriverId,
                DepartmentId = request.DepartmentId,
                PermitTypeId = request.PermitTypeId,
                CommodityTypeId = request.CommodityTypeId,
                RequestStatusId = request.RequestStatusId,
                DriverNationalCardPhoto = request.DriverNationalCardPhoto,
                DriverPhone = request.DriverPhone,
                PermitNumber = request.PermitNumber,
                IsFood = request.IsFood
            };

            return View(await PopulateDropdownsAsync(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Update item
        public async Task<IActionResult> Update(UpdateSupplierRequestVM update)
        {
            if (!ModelState.IsValid)
            {
                return View(await PopulateDropdownsAsync(update));
            }

            var request = await _supplierRequestRepository.GetByIdAsync(update.Id);

            if (request == null)
            {
                ModelState.AddModelError("طلب المورد", "هذا الطلب غير موجود.");
                return View(await PopulateDropdownsAsync(update));
            }

            request.SupplierId = update.SupplierId;
            request.TruckId = update.TruckId;
            request.DriverId = update.DriverId;
            request.DepartmentId = update.DepartmentId;
            request.PermitTypeId = update.PermitTypeId;
            request.CommodityTypeId = update.CommodityTypeId;
            request.RequestStatusId = update.RequestStatusId;
            request.DriverNationalCardPhoto = update.DriverNationalCardPhoto;
            request.DriverPhone = update.DriverPhone;
            request.PermitNumber = update.PermitNumber;
            request.IsFood = update.IsFood;

            _supplierRequestRepository.Update(request);
            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات طلب المورد بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("طلب المورد", "رقم الطلب مفقود.");
                return View(new SupplierRequestDetailsVM());
            }

            // استخدام الدالة التي تحتوى على Includes لعرض البيانات كاملة في صفحة الحذف
            var request = await _supplierRequestRepository.GetByIdWithDetailsAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError("طلب المورد", "هذا الطلب غير موجود.");
                return View(new SupplierRequestDetailsVM());
            }

            var vm = new SupplierRequestDetailsVM
            {
                Id = request.Id,
                SupplierName = request.Supplier?.Name ?? "غير محدد",
                TruckInfo = $"{request.Truck?.PlateLetter} {request.Truck?.PlateNumber}",
                DriverName = request.Driver?.FullName ?? "غير محدد",
                DriverPhone = request.DriverPhone,
                DepartmentName = request.Department?.Name ?? "غير محدد",
                PermitTypeName = request.PermitType?.Name ?? "غير محدد",
                PermitNumber = request.PermitNumber,
                CommodityTypeName = request.CommodityType?.Name ?? "غير محدد",
                RequestStatusName = request.RequestStatus?.Name ?? "غير محدد",
                IsFood = request.IsFood,
                CreatedByName = request.CreatedBy?.Name ?? "غير محدد"
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _supplierRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                ModelState.AddModelError("طلب المورد", "هذا الطلب غير موجود.");
                return View(new SupplierRequestDetailsVM());
            }

            _supplierRequestRepository.Remove(request);
            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف طلب المورد بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        // special acitons 
        [HttpGet]
        public async Task<IActionResult> CreateTruckWithDriver(int supplierId, int? selectedDriverId, bool autoOpenModal = false)
        {
            ViewBag.AutoOpenModal = autoOpenModal;
            // 1. التحقق من وجود المورد
            var supplier = await _supplierRepository.FindAsync(s => s.Id == supplierId);
            if (supplier == null)
            {
                return NotFound("المورد غير موجود");
            }

            // 2. جلب البيانات من قاعدة البيانات
            var allDrivers = await _driverRepository.GetAllAsync();
            var allTruckTypes = await _truckTypeRepository.GetAllAsync();
            var allSuppliers = await _supplierRepository.GetAllAsync();

            var supplierDrivers = allDrivers
                .Where(d => (d.SupplierRequests != null && d.SupplierRequests.Any(sr => sr.SupplierId == supplierId))
                         || (selectedDriverId.HasValue && d.Id == selectedDriverId.Value))
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName,
                    Selected = selectedDriverId.HasValue && d.Id == selectedDriverId.Value
                }).ToList();

            int initialDriverId = selectedDriverId ?? (supplierDrivers.Any() ? int.Parse(supplierDrivers.First().Value) : 0);

            var model = new TruckWithDriverVM
            {
                SupId = supplierId,
                DriverId = initialDriverId,
                Drivers = supplierDrivers,
                TruckTypes = allTruckTypes
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    }).ToList()
            };

            var allDepartments = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = allDepartments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            // 3. إرسال اسم المورد الحالي وقائمة الموردين/الشركات للـ View
            ViewBag.SupplierName = supplier.Name;
            ViewBag.Companies = allSuppliers.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTruckWithDriver(TruckWithDriverVM create)
        {
            if (create.TruckTypeId <= 0)
            {
                var allTruckTypes = await _truckTypeRepository.GetAllAsync();
                var defaultType = allTruckTypes.FirstOrDefault();
                if (defaultType == null)
                {
                    defaultType = new TruckTypes { Name = "عام" };
                    await _truckTypeRepository.AddAsync(defaultType);
                    await _truckTypeRepository.SaveChangesAsync();
                }
                create.TruckTypeId = defaultType.Id;
            }

            if (!ModelState.IsValid)
            {
                await ReloadTruckWithDriverDataAsync(create);
                return View(create);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError("", "يجب تسجيل الدخول أولاً.");
                await ReloadTruckWithDriverDataAsync(create);
                return View(create);
            }

            var currentUser = await _userRepository.GetByIdAsync(int.Parse(userId));

            if (currentUser is null)
            {
                ModelState.AddModelError("", "لم يتم العثور على المستخدم.");
                await ReloadTruckWithDriverDataAsync(create);
                return View(create);
            }

            // 0. إنشاء السائق إذا تم إدخاله من خلال المودال المباشر (+)
            if (!string.IsNullOrWhiteSpace(create.NewDriverName))
            {
                Driver? targetDriver = null;
                if (!string.IsNullOrWhiteSpace(create.NewDriverNationalId))
                {
                    targetDriver = await _driverRepository.FindAsync(d => d.NationalId == create.NewDriverNationalId);
                }

                if (targetDriver == null)
                {
                    int driverTypeId = 1;

                    // SEC-5: استخدام GUID فريد بدلاً من قيم ثابتة تسبب Unique Constraint Violation
                    var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];
                    targetDriver = new Driver
                    {
                        FullName = create.NewDriverName,
                        NationalId = !string.IsNullOrWhiteSpace(create.NewDriverNationalId)
                            ? create.NewDriverNationalId
                            : $"TEMP{uniqueSuffix}",
                        Phone = !string.IsNullOrWhiteSpace(create.NewDriverPhone)
                            ? create.NewDriverPhone
                            : $"0100{uniqueSuffix}",
                        DeiverTypeId = driverTypeId,
                        CreatedBy = currentUser
                    };

                    await _driverRepository.AddAsync(targetDriver);
                    await _driverRepository.SaveChangesAsync();
                }

                create.DriverId = targetDriver.Id;
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

            // 2. إنشاء طلب توريد (SupplierRequest)
            int targetDepartmentId = create.DepartmentId.HasValue && create.DepartmentId.Value > 0 ? create.DepartmentId.Value : 1;

            // BL-2: جلب القيم الافتراضية بالاسم بدلاً من ID=1 الثابتة
            var defaultPermit = await _permitTypeRepository.FindAsync(p => true);
            var defaultCommodity = await _commodityTypeRepository.FindAsync(c => true);
            var defaultStatus = await _requestStatusRepository.FindAsync(s => true);

            var supplierRequest = new SupplierRequest
            {
                SupplierId = create.SupId,
                TruckId = truck.Id,
                DriverId = create.DriverId,
                DepartmentId = targetDepartmentId,
                PermitTypeId = defaultPermit?.Id ?? 1,
                CommodityTypeId = defaultCommodity?.Id ?? 1,
                RequestStatusId = defaultStatus?.Id ?? 1,
                CreatedBy = currentUser
            };

            await _supplierRequestRepository.AddAsync(supplierRequest);
            await _supplierRequestRepository.SaveChangesAsync();

            // 3. إنشاء دور / تذكرة دور (QueueTicket) مع مراعاة إعدادات الـ Reset
            var department = await _departmentRepository.GetByIdAsync(targetDepartmentId);
            var prefix = department?.Prefix ?? "A";

            var settings = await _queueSettingsRepository.FindAsync(x => true);
            DateTimeOffset resetDate = DateTimeOffset.MinValue;

            if (settings != null)
            {
                switch (settings.ResetType)
                {
                    case ResetType.Daily:
                        resetDate = DateTimeOffset.Now.Date;
                        break;
                    case ResetType.ByShift:
                        if (settings.ShiftId.HasValue)
                        {
                            var shift = await _shiftRepository.GetByIdAsync(settings.ShiftId.Value);
                            if (shift != null)
                            {
                                resetDate = DateTime.Today.Add(shift.StartTime);
                                if (shift.LastResetAt.HasValue && shift.LastResetAt > resetDate)
                                    resetDate = shift.LastResetAt.Value;
                            }
                        }
                        break;
                    case ResetType.Manual:
                        resetDate = settings.LastGlobalResetAt ?? DateTimeOffset.MinValue;
                        break;
                }

                if (settings.LastGlobalResetAt.HasValue && settings.LastGlobalResetAt > resetDate)
                    resetDate = settings.LastGlobalResetAt.Value;
            }

            if (department?.LastResetAt.HasValue == true && department.LastResetAt.Value > resetDate)
                resetDate = department.LastResetAt.Value;

            var allTickets = await _ticketRepository.GetAllAsync();
            var lastTicket = allTickets
                .Where(x => x.DepartmentId == targetDepartmentId && x.CreatedAT >= resetDate)
                .OrderByDescending(x => x.CreatedAT)
                .FirstOrDefault();

            int counter = 1;
            if (lastTicket != null)
            {
                var digits = new string(lastTicket.TicketNumber
                    .Where(char.IsDigit)
                    .ToArray());

                if (!string.IsNullOrWhiteSpace(digits) && int.TryParse(digits, out var parsedCounter))
                    counter = parsedCounter + 1;
            }

            var ticketNumber = $"{prefix}{counter}";

            var allTicketStatuses = await _ticketStatusRepository.GetAllAsync();
            var status = allTicketStatuses.FirstOrDefault(s => s.Name.Contains("انتظار") || s.Name.Contains("إنتظار")) ?? allTicketStatuses.FirstOrDefault();
            int statusId = status?.Id ?? 1;

            var queueTicket = new QueueTicket
            {
                TicketNumber = ticketNumber,
                DepartmentId = targetDepartmentId,
                TicketStatusId = statusId,
                SupplierRequestId = supplierRequest.Id,
                QueueTime = DateTimeOffset.Now,
                EntryTime = DateTimeOffset.Now,
                ExitTime = DateTimeOffset.MinValue,
                ShiftId = settings?.ResetType == ResetType.ByShift ? settings.ShiftId : null,
                CreatedBy = currentUser
            };

            await _ticketRepository.AddAsync(queueTicket);
            await _ticketRepository.SaveChangesAsync();

            // 4. التوجيه لـ ViewRole لعرض الأدوار الحالية
            return RedirectToAction("ViewRole", "Authentication");
        }

        #region Helpers

        private async Task<T> PopulateDropdownsAsync<T>(T vm) where T : class
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            var trucks = await _truckRepository.GetAllAsync();
            var drivers = await _driverRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var permitTypes = await _permitTypeRepository.GetAllAsync();
            var commodityTypes = await _commodityTypeRepository.GetAllAsync();
            var requestStatuses = await _requestStatusRepository.GetAllAsync();

            if (vm is CreateSupplierRequestVM createVm)
            {
                createVm.Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                createVm.Trucks = trucks.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.PlateLetter} {t.PlateNumber}" });
                createVm.Drivers = drivers.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName });
                createVm.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
                createVm.PermitTypes = permitTypes.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
                createVm.CommodityTypes = commodityTypes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                createVm.RequestStatuses = requestStatuses.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            }
            else if (vm is UpdateSupplierRequestVM updateVm)
            {
                updateVm.Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                updateVm.Trucks = trucks.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.PlateLetter} {t.PlateNumber}" });
                updateVm.Drivers = drivers.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName });
                updateVm.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
                updateVm.PermitTypes = permitTypes.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
                updateVm.CommodityTypes = commodityTypes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                updateVm.RequestStatuses = requestStatuses.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            }

            return vm;
        }

        private async Task PopulateTruckTypesViewBagAsync(int? selectedTruckTypeId = null)
        {
            var truckTypes = await _truckTypeRepository.GetAllAsync();

            ViewBag.TruckTypes = new SelectList(
                truckTypes,
                nameof(TruckTypes.Id),
                nameof(TruckTypes.Name),
                selectedTruckTypeId
            );
        }

        private async Task LoadDriversAsync(int? selectedDriverId = null)
        {
            var drivers = await _driverRepository.GetAllAsync();

            ViewBag.Drivers = new SelectList(
                drivers,
                "Id",
                "FullName",
                selectedDriverId
            );
        }

        private async Task LoadTruckTypesAsync(int? selectedTruckTypeId = null)
        {
            var truckTypes = await _truckTypeRepository.GetAllAsync();

            ViewBag.TruckTypes = new SelectList(
                truckTypes,
                nameof(TruckTypes.Id),
                nameof(TruckTypes.Name),
                selectedTruckTypeId
            );
        }

        private async Task ReloadTruckWithDriverDataAsync(TruckWithDriverVM create)
        {
            var supplier = await _supplierRepository.FindAsync(s => s.Id == create.SupId);
            ViewBag.SupplierName = supplier?.Name;

            var allDepartments = await _departmentRepository.GetAllAsync();
            ViewBag.Departments = allDepartments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            var allSuppliers = await _supplierRepository.GetAllAsync();
            ViewBag.Companies = allSuppliers.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();

            var allDrivers = await _driverRepository.GetAllAsync();
            create.Drivers = allDrivers
                .Where(d => (d.SupplierRequests != null && d.SupplierRequests.Any(sr => sr.SupplierId == create.SupId))
                         || d.Id == create.DriverId)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName,
                    Selected = d.Id == create.DriverId
                }).ToList();

            var allTruckTypes = await _truckTypeRepository.GetAllAsync();
            create.TruckTypes = allTruckTypes
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();
        }
        #endregion
    }
}
