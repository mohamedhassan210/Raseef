using Rassef.ViewModels.SupplierRequest;

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

        public SupplierRequestController(
            ISupplierRequestRepository supplierRequestRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Department> departmentRepository,
            IRepository<PermitTypes> permitTypeRepository,
            IRepository<CommodityTypes> commodityTypeRepository,
            IRepository<RequestStatuses> requestStatusRepository,
            IRepository<User> userRepository)
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
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requestsRepo =
                await _supplierRequestRepository.GetAllWithDetailsAsync();

            var requests = requestsRepo.Select(x => new SupplierRequestListVM
            {
                Id = x.Id,
                SupplierName = x.Supplier?.Name ?? "غير محدد",
                TruckPlateNumber =
                    $"{x.Truck?.PlateLetter} {x.Truck?.PlateNumber}",
                DriverName = x.Driver?.FullName ?? "غير محدد",
                DriverPhone = x.DriverPhone,
                DepartmentName = x.Department?.Name ?? "غير محدد",
                RequestStatusName = x.RequestStatus?.Name ?? "غير محدد",
                PermitNumber = x.PermitNumber,
            }).ToList();

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "رقم الطلب مفقود."
                );

                return View(new SupplierRequestDetailsVM());
            }

            var request =
                await _supplierRequestRepository
                    .GetByIdWithDetailsAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "هذا الطلب غير موجود."
                );

                return View(new SupplierRequestDetailsVM());
            }

            var requestDetails = new SupplierRequestDetailsVM
            {
                Id = request.Id,
                SupplierName = request.Supplier?.Name ?? "غير محدد",
                TruckInfo =
                    $"{request.Truck?.PlateLetter} {request.Truck?.PlateNumber}",
                DriverName = request.Driver?.FullName ?? "غير محدد",
                DriverPhone = request.DriverPhone,
                DriverNationalCardPhoto =
                    request.DriverNationalCardPhoto,
                DepartmentName =
                    request.Department?.Name ?? "غير محدد",
                PermitTypeName =
                    request.PermitType?.Name ?? "غير محدد",
                PermitNumber = request.PermitNumber,
                CommodityTypeName =
                    request.CommodityType?.Name ?? "غير محدد",
                RequestStatusName =
                    request.RequestStatus?.Name ?? "غير محدد",
                IsFood = request.IsFood,
                CreatedByName =
                    request.CreatedBy?.Name ?? "النظام"
            };

            return View(requestDetails);
        }

        // =====================================================
        // Create Supplier Request
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm =
                await PopulateDropdownsAsync(
                    new CreateSupplierRequestVM()
                );

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateSupplierRequestVM create)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    await PopulateDropdownsAsync(create)
                );
            }

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError(
                    "",
                    "يجب تسجيل الدخول أولاً."
                );

                return View(
                    await PopulateDropdownsAsync(create)
                );
            }

            var currentUser =
                await _userRepository.GetByIdAsync(
                    int.Parse(userId)
                );

            if (currentUser is null)
            {
                ModelState.AddModelError(
                    "",
                    "لم يتم العثور على المستخدم."
                );

                return View(
                    await PopulateDropdownsAsync(create)
                );
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
                DriverNationalCardPhoto =
                    create.DriverNationalCardPhoto,
                DriverPhone = create.DriverPhone,
                PermitNumber = create.PermitNumber,
                IsFood = create.IsFood,
                CreatedBy = currentUser
            };

            await _supplierRequestRepository.AddAsync(request);
            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] =
                "تم إضافة طلب المورد بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // Create Driver With Supplier
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> CreateDriver()
        {
            var vm = new CreateDriverWithSupplierVM
            {
                Suppliers = await GetSuppliersAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDriver(
            CreateDriverWithSupplierVM create)
        {
            if (!ModelState.IsValid)
            {
                create.Suppliers =
                    await GetSuppliersAsync();

                return View(create);
            }

            if (await _driverRepository.ExistsAsync(
                x => x.NationalId == create.NationalId))
            {
                ModelState.AddModelError(
                    nameof(create.NationalId),
                    "الرقم القومي مسجل بالفعل."
                );

                create.Suppliers =
                    await GetSuppliersAsync();

                return View(create);
            }

            if (await _driverRepository.ExistsAsync(
                x => x.Phone == create.Phone))
            {
                ModelState.AddModelError(
                    nameof(create.Phone),
                    "رقم الهاتف مسجل بالفعل."
                );

                create.Suppliers =
                    await GetSuppliersAsync();

                return View(create);
            }

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                ModelState.AddModelError(
                    "",
                    "يجب تسجيل الدخول أولاً."
                );

                create.Suppliers =
                    await GetSuppliersAsync();

                return View(create);
            }

            var currentUser =
                await _userRepository.GetByIdAsync(
                    int.Parse(userId)
                );

            if (currentUser is null)
            {
                ModelState.AddModelError(
                    "",
                    "لم يتم العثور على المستخدم."
                );

                create.Suppliers =
                    await GetSuppliersAsync();

                return View(create);
            }

            var driver = new Driver
            {
                FullName = create.FullName,
                NationalId = create.NationalId,
                Phone = create.Phone,
                CreatedById = currentUser.Id,
                CreatedBy = currentUser
            };

            await _driverRepository.AddAsync(driver);
            await _driverRepository.SaveChangesAsync();

            TempData["Success"] =
                "تم إضافة السائق بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // Update Supplier Request
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "رقم الطلب مفقود."
                );

                return View(
                    await PopulateDropdownsAsync(
                        new UpdateSupplierRequestVM()
                    )
                );
            }

            var request =
                await _supplierRequestRepository
                    .GetByIdAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "هذا الطلب غير موجود."
                );

                return View(
                    await PopulateDropdownsAsync(
                        new UpdateSupplierRequestVM()
                    )
                );
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
                DriverNationalCardPhoto =
                    request.DriverNationalCardPhoto,
                DriverPhone = request.DriverPhone,
                PermitNumber = request.PermitNumber,
                IsFood = request.IsFood
            };

            return View(
                await PopulateDropdownsAsync(vm)
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            UpdateSupplierRequestVM update)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    await PopulateDropdownsAsync(update)
                );
            }

            var request =
                await _supplierRequestRepository
                    .GetByIdAsync(update.Id);

            if (request == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "هذا الطلب غير موجود."
                );

                return View(
                    await PopulateDropdownsAsync(update)
                );
            }

            request.SupplierId = update.SupplierId;
            request.TruckId = update.TruckId;
            request.DriverId = update.DriverId;
            request.DepartmentId = update.DepartmentId;
            request.PermitTypeId = update.PermitTypeId;
            request.CommodityTypeId =
                update.CommodityTypeId;
            request.RequestStatusId =
                update.RequestStatusId;
            request.DriverNationalCardPhoto =
                update.DriverNationalCardPhoto;
            request.DriverPhone = update.DriverPhone;
            request.PermitNumber = update.PermitNumber;
            request.IsFood = update.IsFood;

            _supplierRequestRepository.Update(request);

            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] =
                "تم تحديث بيانات طلب المورد بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // Delete Supplier Request
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "رقم الطلب مفقود."
                );

                return View(
                    new SupplierRequestDetailsVM()
                );
            }

            var request =
                await _supplierRequestRepository
                    .GetByIdWithDetailsAsync(id.Value);

            if (request == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "هذا الطلب غير موجود."
                );

                return View(
                    new SupplierRequestDetailsVM()
                );
            }

            var vm = new SupplierRequestDetailsVM
            {
                Id = request.Id,
                SupplierName =
                    request.Supplier?.Name ?? "غير محدد",
                TruckInfo =
                    $"{request.Truck?.PlateLetter} {request.Truck?.PlateNumber}",
                DriverName =
                    request.Driver?.FullName ?? "غير محدد",
                DriverPhone = request.DriverPhone,
                DepartmentName =
                    request.Department?.Name ?? "غير محدد",
                PermitTypeName =
                    request.PermitType?.Name ?? "غير محدد",
                PermitNumber = request.PermitNumber,
                CommodityTypeName =
                    request.CommodityType?.Name ?? "غير محدد",
                RequestStatusName =
                    request.RequestStatus?.Name ?? "غير محدد",
                IsFood = request.IsFood,
                CreatedByName =
                    request.CreatedBy?.Name ?? "غير محدد"
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var request =
                await _supplierRequestRepository
                    .GetByIdAsync(id);

            if (request == null)
            {
                ModelState.AddModelError(
                    "طلب المورد",
                    "هذا الطلب غير موجود."
                );

                return View(
                    new SupplierRequestDetailsVM()
                );
            }

            _supplierRequestRepository.Remove(request);

            await _supplierRequestRepository.SaveChangesAsync();

            TempData["Success"] =
                "تم حذف طلب المورد بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private async Task<T> PopulateDropdownsAsync<T>(
            T vm) where T : class
        {
            var suppliers =
                await _supplierRepository.GetAllAsync();

            var trucks =
                await _truckRepository.GetAllAsync();

            var drivers =
                await _driverRepository.GetAllAsync();

            var departments =
                await _departmentRepository.GetAllAsync();

            var permitTypes =
                await _permitTypeRepository.GetAllAsync();

            var commodityTypes =
                await _commodityTypeRepository.GetAllAsync();

            var requestStatuses =
                await _requestStatusRepository.GetAllAsync();

            if (vm is CreateSupplierRequestVM createVm)
            {
                createVm.Suppliers =
                    suppliers.Select(s =>
                        new SelectListItem
                        {
                            Value = s.Id.ToString(),
                            Text = s.Name
                        });

                createVm.Trucks =
                    trucks.Select(t =>
                        new SelectListItem
                        {
                            Value = t.Id.ToString(),
                            Text =
                                $"{t.PlateLetter} {t.PlateNumber}"
                        });

                createVm.Drivers =
                    drivers.Select(d =>
                        new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.FullName
                        });

                createVm.Departments =
                    departments.Select(d =>
                        new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.Name
                        });

                createVm.PermitTypes =
                    permitTypes.Select(p =>
                        new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.Name
                        });

                createVm.CommodityTypes =
                    commodityTypes.Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        });

                createVm.RequestStatuses =
                    requestStatuses.Select(r =>
                        new SelectListItem
                        {
                            Value = r.Id.ToString(),
                            Text = r.Name
                        });
            }
            else if (vm is UpdateSupplierRequestVM updateVm)
            {
                updateVm.Suppliers =
                    suppliers.Select(s =>
                        new SelectListItem
                        {
                            Value = s.Id.ToString(),
                            Text = s.Name
                        });

                updateVm.Trucks =
                    trucks.Select(t =>
                        new SelectListItem
                        {
                            Value = t.Id.ToString(),
                            Text =
                                $"{t.PlateLetter} {t.PlateNumber}"
                        });

                updateVm.Drivers =
                    drivers.Select(d =>
                        new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.FullName
                        });

                updateVm.Departments =
                    departments.Select(d =>
                        new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.Name
                        });

                updateVm.PermitTypes =
                    permitTypes.Select(p =>
                        new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.Name
                        });

                updateVm.CommodityTypes =
                    commodityTypes.Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        });

                updateVm.RequestStatuses =
                    requestStatuses.Select(r =>
                        new SelectListItem
                        {
                            Value = r.Id.ToString(),
                            Text = r.Name
                        });
            }

            return vm;
        }

        private async Task<IEnumerable<SelectListItem>>
            GetSuppliersAsync()
        {
            var suppliers =
                await _supplierRepository.GetAllAsync();

            return suppliers.Select(s =>
                new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                });
        }

        #endregion
    }
}