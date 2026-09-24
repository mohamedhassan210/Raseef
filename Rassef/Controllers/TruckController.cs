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
        private readonly IRepository<DriverTypes> _driverTypeRepository;
        private readonly IRepository<Dock> _dockRepository;
        private readonly IRepository<DockAssignment> _dockAssignmentRepository;
        private readonly ITicketEngineService _ticketEngineService;
        private readonly Rassef.Common.Interfaces.IDockAvailabilityService _dockAvailabilityService;

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
            IRepository<DriverTypes> driverTypeRepository,
            IRepository<Dock> dockRepository,
            IRepository<DockAssignment> dockAssignmentRepository,
            ITicketEngineService ticketEngineService,
            Rassef.Common.Interfaces.IDockAvailabilityService dockAvailabilityService)
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
            _driverTypeRepository = driverTypeRepository;
            _dockRepository = dockRepository;
            _dockAssignmentRepository = dockAssignmentRepository;
            _ticketEngineService = ticketEngineService;
            _dockAvailabilityService = dockAvailabilityService;
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

            // CORRECTED (per explicit instruction from the project owner):
            // the supplier request flow only offers EXTERNAL trucks — TruckTypeCode == 2 —
            // not code == 1. A prior pass had this backwards.
            var supplierTrucks = trucksrepo
                .Where(x => !activeTruckIds.Contains(x.Id) && x.TruckType?.TruckTypeCode == 2)
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
            ViewBag.Search = searchString;

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
            var allTrucks = await _truckRepository.GetAllAsync(query => query.Include(t => t.TruckType));

            // فلو التحويل بيعرض شاحنات داخلية بس — TruckTypeCode == 1
            // (النوع == 2 ده بتاع فلو التوريد الخارجي)
            var truckList = allTrucks
                .Where(d => !activeTruckIds.Contains(d.Id) && d.TruckType?.TruckTypeCode == 1)
                .Select(d => new TruckListVM
                {
                    Id = d.Id,
                    PlateNumber = d.PlateNumber,
                    PlateLetter = d.PlateLetter,
                    StorageCapacity = d.StorageCapacity,
                    IsRefrigerated = d.IsRefrigerated
                }).ToList();

            var depts = await GetScopedDepartmentsAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();

            return View(truckList);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> AddTraDriver(int? supplierId)
        {
            var suppliers = await GetSuppliersAsync();
            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            // فلو التحويل بيعرض سواقين داخليين بس — DriverTypes.Code == 1
            var allDrivers = await _driverRepository.GetAllAsync(query => query.Include(d => d.DeiverType));
            var availableDrivers = allDrivers.Where(d => !activeDriverIds.Contains(d.Id) && d.DeiverType?.Code == 1).ToList();
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

            var allDocksInfo = await _dockAvailabilityService.GetAllDocksAsync();
            model.AllDocks = allDocksInfo.Select(d => new Rassef.ViewModels.Dock.DockOptionVM
            {
                Id = d.Id,
                DockName = d.DockName,
                DepartmentId = d.DepartmentId,
                IsUnderMaintenance = d.IsUnderMaintenance,
                Occupancy = d.Occupancy,
                MaxTruckCount = d.MaxTruckCount
            });

            var depts = await GetScopedDepartmentsAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();

            // NEW — backs the permit-type dropdown added to the department/dock modal.
            var permitTypesForView = await _permitTypeRepository.GetAllAsync();
            model.PermitTypes = permitTypesForView.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();

            ViewBag.Suppliers = suppliers;
            ViewBag.SupplierName = supplierName;

            return View(model);
        }

        // NEW — backs the driver search box on AddTraDriver (the page was missing one
        // entirely). Mirrors SupplierRequestController.SearchDrivers exactly, except it
        // filters to INTERNAL drivers (DeiverType.Code == 1) instead of external
        // (Code == 2), matching the rest of the transfer flow.
        [HttpGet]
        public async Task<IActionResult> SearchDrivers(string? term)
        {
            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();

            IReadOnlyList<Driver> drivers;

            if (string.IsNullOrWhiteSpace(term))
            {
                drivers = await _driverRepository.GetAllAsync(q => q
                    .Include(d => d.DeiverType)
                    .Where(d => !d.IsDeleted && !activeDriverIds.Contains(d.Id) && d.DeiverType.Code == 1)
                    .OrderByDescending(d => d.CreatedAT)
                    .ThenByDescending(d => d.Id)
                    .Take(6));
            }
            else
            {
                var trimmedTerm = term.Trim();
                drivers = await _driverRepository.GetAllAsync(q => q
                    .Include(d => d.DeiverType)
                    .Where(d => !d.IsDeleted && !activeDriverIds.Contains(d.Id) && d.DeiverType.Code == 1 &&
                        (d.FullName.Contains(trimmedTerm) ||
                         d.Phone.Contains(trimmedTerm) ||
                         d.NationalId.Contains(trimmedTerm)))
                    .OrderByDescending(d => d.CreatedAT)
                    .ThenByDescending(d => d.Id)
                    .Take(20));
            }

            var result = drivers.Select(d => new DriverListVM
            {
                Id = d.Id,
                FullName = d.FullName,
                NationalId = d.NationalId,
                Phone = d.Phone
            });

            return Json(result);
        }

        // NEW — factored out of the several `return View(create)` branches below. Each
        // of those previously only reset create.Suppliers/ViewBag.Suppliers/
        // ViewBag.SupplierName before redisplaying the form, leaving create.Drivers,
        // create.AllDocks and ViewBag.Departments empty — so a validation error (e.g.
        // "dock already full") would redisplay the page with an empty driver dropdown,
        // no docks and no departments at all. This repopulates everything the GET
        // action populates, so a redisplay after a validation error looks the same as
        // a fresh load.
        private async Task RepopulateAddTraDriverFormAsync(Rassef.ViewModels.Truck.AddTraTruckVM create)
        {
            var (activeDriverIds, _) = await GetActiveDriverAndTruckIdsAsync();
            var allDrivers = await _driverRepository.GetAllAsync(query => query.Include(d => d.DeiverType));
            var availableDrivers = allDrivers.Where(d => !activeDriverIds.Contains(d.Id) && d.DeiverType?.Code == 1).ToList();
            create.Drivers = availableDrivers.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName }).ToList();

            var allDocksInfo = await _dockAvailabilityService.GetAllDocksAsync();
            create.AllDocks = allDocksInfo.Select(d => new Rassef.ViewModels.Dock.DockOptionVM
            {
                Id = d.Id,
                DockName = d.DockName,
                DepartmentId = d.DepartmentId,
                IsUnderMaintenance = d.IsUnderMaintenance,
                Occupancy = d.Occupancy,
                MaxTruckCount = d.MaxTruckCount
            });

            create.Suppliers = await GetSuppliersAsync();
            ViewBag.Suppliers = create.Suppliers;
            ViewBag.SupplierName = create.SupplierName;

            var depts = await GetScopedDepartmentsAsync();
            ViewBag.Departments = depts.Select(d => new { id = d.Id, name = d.Name }).ToList();

            var permitTypesForView = await _permitTypeRepository.GetAllAsync();
            create.PermitTypes = permitTypesForView.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
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
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }
            }

            if (create.DriverId.HasValue && create.DriverId.Value > 0 && activeDriverIds.Contains(create.DriverId.Value))
            {
                ModelState.AddModelError("DriverId", "السائق المختار لديه دور نشط حالياً. يجب اكتمال الدور السابق أولاً.");
                await RepopulateAddTraDriverFormAsync(create);
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
                // CHANGED — same class of bug already fixed on the supplier side: was
                // `TruckTypeId = 1`, a hardcoded FK assuming the internal truck type's
                // database Id happens to be 1. Now looked up by TruckTypeCode == 1
                // (فلو التحويل بيعتبر أي شاحنة جديدة "داخلية" بشكل افتراضي)، مع سقوط آمن
                // على أول نوع موجود لو مفيش نوع بكود 1 أصلاً.
                var allTruckTypesForNewTruck = await _truckTypeRepository.GetAllAsync();
                var internalTruckType = allTruckTypesForNewTruck.FirstOrDefault(t => t.TruckTypeCode == 1)
                    ?? allTruckTypesForNewTruck.FirstOrDefault();

                if (internalTruckType == null)
                {
                    ModelState.AddModelError("", "لا توجد أنواع شاحنات مُعرّفة في النظام. يرجى إضافة نوع شاحنة أولاً قبل المتابعة.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                truck = new Truck
                {
                    PlateNumber = plateNum,
                    PlateLetter = plateLet,
                    StorageCapacity = create.StorageCapacity ?? 0,
                    IsRefrigerated = create.TruckType == "تبريد",
                    TruckTypeId = internalTruckType.Id,
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
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }
                if (driver == null)
                {
                    // فلو التحويل بيعتبر أي سائق جديد "داخلي" (Code == 1) بشكل افتراضي
                    var internalDriverType = (await _driverTypeRepository.GetAllAsync())
                        .FirstOrDefault(t => t.Code == 1) ?? (await _driverTypeRepository.GetAllAsync()).FirstOrDefault();

                    driver = new Driver
                    {
                        FullName = create.NewDriverName,
                        NationalId = create.NewDriverNationalId,
                        Phone = create.NewDriverPhone,
                        DeiverTypeId = internalDriverType?.Id ?? 0,
                        CreatedById = currentUserId
                    };
                    await _driverRepository.AddAsync(driver);
                    await _driverRepository.SaveChangesAsync();
                }
            }

            // CHANGED — same class of bug as the truck/permit/status fallbacks above: was
            // "any driver at all, or hardcode Id == 1" if nothing else was picked. Now
            // scoped to internal drivers (Code == 1) like the rest of this flow, and
            // surfaces a validation error instead of silently guessing driver 1.
            int finalDriverId = driver?.Id ?? (create.DriverId.HasValue && create.DriverId.Value > 0 ? create.DriverId.Value : 0);
            if (finalDriverId <= 0)
            {
                var internalDriversForFallback = await _driverRepository.GetAllAsync(q => q.Include(d => d.DeiverType));
                var fallbackDriver = internalDriversForFallback.FirstOrDefault(d => !activeDriverIds.Contains(d.Id) && d.DeiverType?.Code == 1);
                if (fallbackDriver == null)
                {
                    ModelState.AddModelError("DriverId", "يرجى اختيار سائق أو إضافة سائق جديد.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }
                finalDriverId = fallbackDriver.Id;
            }

            if (create.DepartmentId.HasValue && create.DepartmentId.Value > 0)
            {
                // الرصيف بقى إجباري زي فلو التوريد بالظبط
                if (!create.DockId.HasValue || create.DockId.Value <= 0)
                {
                    ModelState.AddModelError("DockId", "يرجى اختيار الرصيف للمتابعة.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                var (isDockValid, dockErrorMessage) = await _dockAvailabilityService.ValidateDockSelectionAsync(create.DockId.Value, create.DepartmentId.Value);
                if (!isDockValid)
                {
                    ModelState.AddModelError("DockId", dockErrorMessage ?? "الرصيف المختار غير متاح.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                // CHANGED — AvizNumber, PermitNumber and PermitTypeId are now real user
                // input from the department/dock modal (TransferRequest already has all
                // three fields) instead of being auto-generated / auto-picked. Same
                // hardcoded-FK-fallback bug class as before if left unchecked, so these
                // are validated explicitly rather than defaulted.
                if (string.IsNullOrWhiteSpace(create.AvizNumber))
                {
                    ModelState.AddModelError("AvizNumber", "يرجى إدخال رقم الأفيز.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                if (string.IsNullOrWhiteSpace(create.PermitNumber))
                {
                    ModelState.AddModelError("PermitNumber", "يرجى إدخال رقم التصريح.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                if (!create.PermitTypeId.HasValue || create.PermitTypeId.Value <= 0)
                {
                    ModelState.AddModelError("PermitTypeId", "يرجى اختيار نوع الإذن.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                var chosenPermitType = await _permitTypeRepository.GetByIdAsync(create.PermitTypeId.Value);
                if (chosenPermitType == null)
                {
                    ModelState.AddModelError("PermitTypeId", "نوع الإذن المختار غير موجود.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                var defaultStatusForTransfer = await _requestStatusRepository.FindAsync(s => true);
                if (defaultStatusForTransfer == null)
                {
                    ModelState.AddModelError("", "بيانات إعداد النظام غير مكتملة (حالة الطلب). يرجى مراجعة الإعدادات.");
                    await RepopulateAddTraDriverFormAsync(create);
                    return View(create);
                }

                var req = new TransferRequest
                {
                    DepartmentId = create.DepartmentId.Value,
                    TruckId = truck.Id,
                    DriverId = finalDriverId,
                    PermitTypeId = chosenPermitType.Id,
                    PermitNumber = create.PermitNumber.Trim(),
                    AvizNumber = create.AvizNumber.Trim(),
                    RequestStatusId = defaultStatusForTransfer.Id,
                    DockId = create.DockId,
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

            if (!dto.DriverId.HasValue || dto.DriverId.Value <= 0)
            {
                return BadRequest(new { success = false, message = "يرجى اختيار سائق قبل المتابعة." });
            }

            var driver = await _driverRepository.GetByIdAsync(dto.DriverId.Value);
            if (driver == null)
            {
                return NotFound(new { success = false, message = "السائق المحدد غير موجود." });
            }

            if (activeDriverIds.Contains(driver.Id))
            {
                return BadRequest(new { success = false, message = "السائق المختار لديه دور نشط حالياً. يجب اكتمال الدور السابق أولاً." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var parsedId))
            {
                return BadRequest(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            var currentUser = await _userRepository.GetByIdAsync(parsedId);
            if (currentUser == null)
            {
                return BadRequest(new { success = false, message = "لم يتم العثور على المستخدم." });
            }

            var defaultPermit = (await _permitTypeRepository.GetAllAsync()).FirstOrDefault();
            var defaultStatus = (await _requestStatusRepository.GetAllAsync()).FirstOrDefault();

            if (defaultPermit == null || defaultStatus == null)
            {
                return BadRequest(new { success = false, message = "بيانات إعداد النظام غير مكتملة (نوع التصريح / حالة الطلب)." });
            }

            // الرصيف بقى إجباري
            if (!dto.DockId.HasValue || dto.DockId.Value <= 0)
            {
                return BadRequest(new { success = false, message = "لازم تختار رصيف قبل تأكيد الطلب." });
            }

            {
                var (isValid, errorMessage) = await _dockAvailabilityService.ValidateDockSelectionAsync(dto.DockId.Value, dto.DepartmentId);
                if (!isValid)
                {
                    return BadRequest(new { success = false, message = errorMessage ?? "الرصيف المختار غير متاح." });
                }
            }

            var allTransfers = await _transferRequestRepository.GetAllAsync();
            int nextAvizNumber = allTransfers.Count() + 1;
            string avizNumber = $"AVIZ-{nextAvizNumber:D4}";

            var transferRequest = new TransferRequest
            {
                TruckId = truck.Id,
                DriverId = driver.Id,
                DepartmentId = department.Id,
                PermitTypeId = defaultPermit.Id,
                PermitNumber = $"PER-TR-{DateTime.Now.Ticks % 100000}",
                AvizNumber = avizNumber,
                RequestStatusId = defaultStatus.Id,
                DockId = dto.DockId is > 0 ? dto.DockId : null,
                CreatedById = currentUser.Id.ToString(),
                CreatedBy = currentUser
            };

            await _transferRequestRepository.AddAsync(transferRequest);
            await _transferRequestRepository.SaveChangesAsync();

            var ticketResult = await _ticketEngineService.IssueTransferTicketAsync(department.Id, transferRequest.Id, currentUser.Id);

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

        /// <summary>
        /// Feature 3 — the departments offered to the user are limited to the
        /// warehouse they are currently working in (the SelectedWarehouseId cookie).
        /// A null selection means "unresolved" (user hasn't picked a warehouse yet),
        /// which falls through unfiltered rather than presenting an empty dropdown.
        /// Soft-deleted departments are excluded here too — the previous plain
        /// GetAllAsync() call had no IsDeleted filter at all.
        /// </summary>
        private async Task<IReadOnlyList<Department>> GetScopedDepartmentsAsync()
        {
            var selectedWarehouseId = Request.GetSelectedWarehouseId();

            return await _departmentRepository.GetAllAsync(q => q
                .Where(d => !d.IsDeleted)
                .Where(d => selectedWarehouseId == null || d.WarehouseId == selectedWarehouseId.Value));
        }

    }
}