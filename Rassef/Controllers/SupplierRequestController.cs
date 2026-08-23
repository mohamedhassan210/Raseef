namespace Rassef.Controllers
{
    public class SupplierRequestController : Controller
    {
        private readonly ISupplierRequestRepository _supplierRequestRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IDriverRepository _driverRepository;
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
        private readonly IRepository<DriverTypes> _driverTypeRepository;
        private readonly ITicketEngineService _ticketEngineService;

        public SupplierRequestController(
            ISupplierRequestRepository supplierRequestRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<Truck> truckRepository,
            IDriverRepository driverRepository,
            IRepository<Department> departmentRepository,
            IRepository<PermitTypes> permitTypeRepository,
            IRepository<CommodityTypes> commodityTypeRepository,
            IRepository<RequestStatuses> requestStatusRepository,
            IRepository<User> userRepository,
            IRepository<TruckTypes> truckTypeRepository,
            IRepository<DriverTypes> driverTypeRepository,
            IRepository<QueueTicket> ticketRepository,
            IRepository<TicketStatuses> ticketStatusRepository,
            IRepository<QueueSettings> queueSettingsRepository,
            IRepository<Shift> shiftRepository,
            ITicketEngineService ticketEngineService)
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
            _driverTypeRepository = driverTypeRepository;
            _ticketRepository = ticketRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _shiftRepository = shiftRepository;
            _ticketEngineService = ticketEngineService;
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

            // 2. جلب السائقين والشاحنات المشغولة حالياً بأدوار نشطة (انتظار أو جاري)
            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            // 3. جلب السائقين التابعين لهذا المورد تحديداً واستبعاد من لديه دور نشط
            var supplierDriversEntities = (await _driverRepository.GetDriversBySupplierIdAsync(supplierId)).ToList();
            if (selectedDriverId.HasValue && !supplierDriversEntities.Any(d => d.Id == selectedDriverId.Value))
            {
                var selDriver = await _driverRepository.GetByIdAsync(selectedDriverId.Value);
                if (selDriver != null)
                {
                    supplierDriversEntities.Add(selDriver);
                }
            }

            // استبعاد السائقين الذين لديهم أدوار نشطة لم تكتمل بعد
            supplierDriversEntities = supplierDriversEntities
                .Where(d => !activeDriverIds.Contains(d.Id) || (selectedDriverId.HasValue && d.Id == selectedDriverId.Value))
                .ToList();

            var supplierDrivers = supplierDriversEntities
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName,
                    Selected = selectedDriverId.HasValue && d.Id == selectedDriverId.Value
                }).ToList();

            var allTruckTypes = await _truckTypeRepository.GetAllAsync();
            var allSuppliers = await _supplierRepository.GetAllAsync();

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

            // 4. إرسال اسم المورد الحالي وقائمة الموردين/الشركات للـ View
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

            // جلب السائقين والشاحنات المشغولة حالياً بأدوار نشطة
            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            // 0. إنشاء أو ربط السائق إذا تم إدخاله من خلال المودال المباشر (+)
            if (!string.IsNullOrWhiteSpace(create.NewDriverName))
            {
                var newName = create.NewDriverName.Trim();
                var newNatId = create.NewDriverNationalId?.Trim() ?? "";
                var newPhone = create.NewDriverPhone?.Trim() ?? "";

                // 1. التحقق من صحة الرقم القومي (14 رقم)
                if (string.IsNullOrWhiteSpace(newNatId) || newNatId.Length != 14 || !newNatId.All(char.IsDigit))
                {
                    ModelState.AddModelError(nameof(create.NewDriverNationalId), "الرقم القومي يجب أن يتكون من 14 رقماً.");
                }

                // 2. التحقق من صحة رقم الهاتف المصري (11 رقم يبدأ بـ 010 أو 011 أو 012 أو 015)
                if (string.IsNullOrWhiteSpace(newPhone) || !System.Text.RegularExpressions.Regex.IsMatch(newPhone, @"^01[0125][0-9]{8}$"))
                {
                    ModelState.AddModelError(nameof(create.NewDriverPhone), "يرجى إدخال رقم هاتف مصري صحيح (11 رقماً يبدأ بـ 010 أو 011 أو 012 أو 015).");
                }

                if (!ModelState.IsValid)
                {
                    await ReloadTruckWithDriverDataAsync(create);
                    return View(create);
                }

                // 3. التحقق إذا كان السائق مسجلاً مسبقاً بنفس الرقم القومي أو الهاتف
                var existingDriver = await _driverRepository.FindAsync(d => (d.NationalId == newNatId || d.Phone == newPhone) && !d.IsDeleted);

                if (existingDriver != null)
                {
                    if (activeDriverIds.Contains(existingDriver.Id))
                    {
                        ModelState.AddModelError(nameof(create.NewDriverNationalId), $"السائق ({existingDriver.FullName}) لديه دور حالي في الانتظار أو قيد التنفيذ. يجب اكتمال الدور السابق أولاً.");
                        await ReloadTruckWithDriverDataAsync(create);
                        return View(create);
                    }
                    create.DriverId = existingDriver.Id;
                }
                else
                {
                    var allDriverTypes = await _driverTypeRepository.GetAllAsync();
                    var defaultType = allDriverTypes.FirstOrDefault();
                    int driverTypeId = defaultType?.Id ?? 1;

                    var newDriver = new Driver
                    {
                        FullName = newName,
                        NationalId = newNatId,
                        Phone = newPhone,
                        DeiverTypeId = driverTypeId,
                        CreatedBy = currentUser
                    };

                    await _driverRepository.AddAsync(newDriver);
                    await _driverRepository.SaveChangesAsync();

                    create.DriverId = newDriver.Id;
                }
            }

            // التحقق من أن السائق المختار ليس لديه دور نشط
            if (create.DriverId > 0 && activeDriverIds.Contains(create.DriverId))
            {
                var busyDriver = await _driverRepository.GetByIdAsync(create.DriverId);
                ModelState.AddModelError(nameof(create.DriverId), $"السائق ({busyDriver?.FullName ?? "المحدد"}) لديه دور حالي في الانتظار أو قيد التنفيذ. يجب اكتمال الدور السابق أولاً.");
                await ReloadTruckWithDriverDataAsync(create);
                return View(create);
            }

            var plateNum = create.PlateNumber?.Trim() ?? "";
            var plateLet = create.PlateLetter?.Trim() ?? "";

            var existingTruck = await _truckRepository.FindAsync(t => t.PlateNumber == plateNum && t.PlateLetter == plateLet && !t.IsDeleted);

            // التحقق من أن الشاحنة ليس لديها دور نشط
            if (existingTruck != null && activeTruckIds.Contains(existingTruck.Id))
            {
                ModelState.AddModelError(nameof(create.PlateNumber), $"الشاحنة ({existingTruck.PlateLetter} {existingTruck.PlateNumber}) لديها دور حالي في الانتظار أو قيد التنفيذ. يجب اكتمال الدور السابق وتسجيل الخروج أولاً.");
                await ReloadTruckWithDriverDataAsync(create);
                return View(create);
            }

            Truck truck;

            if (existingTruck != null)
            {
                truck = existingTruck;
                truck.StorageCapacity = create.StorageCapacity;
                truck.IsRefrigerated = create.IsRefrigerated;
                truck.TruckTypeId = create.TruckTypeId;
                _truckRepository.Update(truck);
                await _truckRepository.SaveChangesAsync();
            }
            else
            {
                truck = new Truck
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
            }

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

            // 3. إنشاء دور / تذكرة دور (QueueTicket) عبر محرك التذاكر
            var ticketResult = await _ticketEngineService.IssueSupplierTicketAsync(targetDepartmentId, supplierRequest.Id, currentUser?.Id ?? 1);

            // 4. التوجيه لـ Recript مع تمرير رقم التذكرة
            return RedirectToAction("Recript", "Driver", new { ticketId = ticketResult.TicketId });
        }

        #region Helpers

        private async Task<T> PopulateDropdownsAsync<T>(T vm) where T : class
        {
            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            var suppliers = await _supplierRepository.GetAllAsync();
            var allTrucks = await _truckRepository.GetAllAsync();
            var allDrivers = await _driverRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var permitTypes = await _permitTypeRepository.GetAllAsync();
            var commodityTypes = await _commodityTypeRepository.GetAllAsync();
            var requestStatuses = await _requestStatusRepository.GetAllAsync();

            var trucks = allTrucks.Where(t => !activeTruckIds.Contains(t.Id)).ToList();
            var drivers = allDrivers.Where(d => !activeDriverIds.Contains(d.Id)).ToList();

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
                updateVm.Trucks = allTrucks.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.PlateLetter} {t.PlateNumber}" });
                updateVm.Drivers = allDrivers.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName });
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

            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            var supplierDriversEntities = (await _driverRepository.GetDriversBySupplierIdAsync(create.SupId)).ToList();
            if (create.DriverId > 0 && !supplierDriversEntities.Any(d => d.Id == create.DriverId))
            {
                var selDriver = await _driverRepository.GetByIdAsync(create.DriverId);
                if (selDriver != null)
                {
                    supplierDriversEntities.Add(selDriver);
                }
            }

            // استبعاد السائقين الذين لديهم أدوار نشطة
            supplierDriversEntities = supplierDriversEntities
                .Where(d => !activeDriverIds.Contains(d.Id) || d.Id == create.DriverId)
                .ToList();

            create.Drivers = supplierDriversEntities
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
        #endregion
    }
}
