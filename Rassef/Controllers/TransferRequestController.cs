namespace Rassef.Controllers
{
    public class TransferRequestController : Controller
    {
        private readonly ITransferRequestRepository _repository;
        private readonly ITruckRepository _truckRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPermitTypeRepository _permitTypeRepository;
        private readonly IRequestStatusRepository _requestStatusRepository;
        private readonly ITicketEngineService _ticketEngineService;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;

        public TransferRequestController(
            ITransferRequestRepository repository,
            ITruckRepository truckRepository,
            IDriverRepository driverRepository,
            IDepartmentRepository departmentRepository,
            IPermitTypeRepository permitTypeRepository,
            IRequestStatusRepository requestStatusRepository,
            ITicketEngineService ticketEngineService,
            IUserRepository userRepository,
            IRepository<QueueTicket> ticketRepository)
        {
            _repository = repository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _departmentRepository = departmentRepository;
            _permitTypeRepository = permitTypeRepository;
            _requestStatusRepository = requestStatusRepository;
            _ticketEngineService = ticketEngineService;
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
        }

        private async Task LoadSelectListsAsync(CreateTransferRequestVM model)
        {
            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            var departments = await _departmentRepository.GetAllAsync();
            var permitTypes = await _permitTypeRepository.GetAllAsync();
            var allTrucks = await _truckRepository.GetAllAsync();
            var allDrivers = await _driverRepository.GetAllAsync();

            var availableTrucks = allTrucks.Where(t => !activeTruckIds.Contains(t.Id)).ToList();
            var availableDrivers = allDrivers.Where(d => !activeDriverIds.Contains(d.Id)).ToList();

            model.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            });

            model.PermitTypes = permitTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            });

            model.Trucks = availableTrucks.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.PlateLetter} {t.PlateNumber} ({(t.IsRefrigerated ? "تبريد" : "غير تبريد")})"
            });

            model.Drivers = availableDrivers.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.FullName
            });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = await _repository.GetAllAsync(
                include: query => query
                    .Include(r => r.Truck)
                    .Include(r => r.Driver)
                    .Include(r => r.Department)
                    .Include(r => r.PermitType)
                    .Include(r => r.RequestStatus)
            );

            var model = requests.Select(request => new TransferRequestDetailsVM
            {
                Id = request.Id,
                AvizNumber = request.AvizNumber,
                Truck = request.Truck != null ? $"{request.Truck.PlateNumber} {request.Truck.PlateLetter}" : "غير محدد",
                Driver = request.Driver != null ? request.Driver.FullName : "غير محدد",
                Department = request.Department != null ? request.Department.Name : "غير محدد",
                PermitType = request.PermitType != null ? request.PermitType.Name : "غير محدد",
                PermitNumber = request.PermitNumber,
                RequestStatus = request.RequestStatus != null ? request.RequestStatus.Name : "غير محدد"
            }).ToList();

            return View(model);
        }

        // Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _repository.GetByIdWithDetailsAsync(id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                return View();
            }

            var model = new TransferRequestDetailsVM
            {
                Id = request.Id,
                AvizNumber = request.AvizNumber,
                Truck = $"{request.Truck.PlateNumber} {request.Truck.PlateLetter}",
                Driver = request.Driver.FullName,
                Department = request.Department.Name,
                PermitType = request.PermitType.Name,
                PermitNumber = request.PermitNumber,
                RequestStatus = request.RequestStatus.Name
            };

            return View(model);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTransferRequestVM();
            await LoadSelectListsAsync(model);
            return View(model);
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransferRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(model);
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.AvizNumber == model.AvizNumber))
            {
                ModelState.AddModelError(nameof(model.AvizNumber), "رقم الأفيز مسجل بالفعل.");
                await LoadSelectListsAsync(model);
                return View(model);
            }

            // استخراج معرف المستخدم الحالي
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 1;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                currentUserId = parsedId;
            }
            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            var (activeDriverIds, activeTruckIds) = await GetActiveDriverAndTruckIdsAsync();

            int finalTruckId = model.TruckId;
            if (finalTruckId <= 0)
            {
                var firstTruck = (await _truckRepository.GetAllAsync()).FirstOrDefault(t => !activeTruckIds.Contains(t.Id));
                finalTruckId = firstTruck?.Id ?? 1;
            }

            int finalDriverId = model.DriverId;
            if (finalDriverId <= 0)
            {
                var firstDriver = (await _driverRepository.GetAllAsync()).FirstOrDefault(d => !activeDriverIds.Contains(d.Id));
                finalDriverId = firstDriver?.Id ?? 1;
            }

            if (activeTruckIds.Contains(finalTruckId))
            {
                ModelState.AddModelError(nameof(model.TruckId), "الشاحنة المختارة لديها دور نشط حالياً (في الانتظار أو قيد التنفيذ). يجب اكتمال الدور السابق أولاً.");
                await LoadSelectListsAsync(model);
                return View(model);
            }

            if (activeDriverIds.Contains(finalDriverId))
            {
                ModelState.AddModelError(nameof(model.DriverId), "السائق المختار لديه دور نشط حالياً (في الانتظار أو قيد التنفيذ). يجب اكتمال الدور السابق أولاً.");
                await LoadSelectListsAsync(model);
                return View(model);
            }

            var request = new TransferRequest
            {
                AvizNumber = !string.IsNullOrWhiteSpace(model.AvizNumber) ? model.AvizNumber : $"AVIZ-{DateTime.Now.Ticks % 10000:D4}",
                PermitNumber = !string.IsNullOrWhiteSpace(model.PermitNumber) ? model.PermitNumber : $"PER-TR-{DateTime.Now.Ticks % 100000}",
                TruckId = finalTruckId,
                DriverId = finalDriverId,
                DepartmentId = model.DepartmentId,
                PermitTypeId = model.PermitTypeId > 0 ? model.PermitTypeId : 1,
                RequestStatusId = 1,
                CreatedById = currentUserId.ToString(),
                CreatedBy = currentUser!
            };

            await _repository.AddAsync(request);
            await _repository.SaveChangesAsync();

            var ticketResult = await _ticketEngineService.IssueTransferTicketAsync(model.DepartmentId, request.Id, currentUserId);

            TempData["Success"] = $"تم حفظ طلب التحويل وإصدار الدور رقم {ticketResult.TicketNumber} بنجاح.";
            return RedirectToAction("Recript", "Driver", new { ticketId = ticketResult.TicketId });
        }

        // Update (GET)
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var request = await _repository.GetByIdWithDetailsAsync(id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                return View();
            }

            var model = new UpdateTransferRequestVM
            {
                Id = request.Id,
                AvizNumber = request.AvizNumber,
                TruckId = request.TruckId,
                DriverId = request.DriverId,
                PermitTypeId = request.PermitTypeId,
                PermitNumber = request.PermitNumber,
                DepartmentId = request.DepartmentId,
                RequestStatusId = request.RequestStatusId
            };

            ViewBag.Departments = await _departmentRepository.GetAllAsync();
            ViewBag.PermitTypes = await _permitTypeRepository.GetAllAsync();
            ViewBag.RequestStatuses = await _requestStatusRepository.GetAllAsync();

            return View(model);
        }

        // Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTransferRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _departmentRepository.GetAllAsync();
                ViewBag.PermitTypes = await _permitTypeRepository.GetAllAsync();
                ViewBag.RequestStatuses = await _requestStatusRepository.GetAllAsync();
                return View(model);
            }

            var request = await _repository.GetByIdAsync(model.Id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.AvizNumber == model.AvizNumber && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.AvizNumber), "رقم الأفيز مسجل بالفعل.");
                ViewBag.Departments = await _departmentRepository.GetAllAsync();
                ViewBag.PermitTypes = await _permitTypeRepository.GetAllAsync();
                ViewBag.RequestStatuses = await _requestStatusRepository.GetAllAsync();
                return View(model);
            }

            request.AvizNumber = model.AvizNumber;
            request.TruckId = model.TruckId;
            request.DriverId = model.DriverId;
            request.PermitTypeId = model.PermitTypeId;
            request.PermitNumber = model.PermitNumber;
            request.DepartmentId = model.DepartmentId;
            request.RequestStatusId = model.RequestStatusId;

            _repository.Update(request);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _repository.GetByIdWithDetailsAsync(id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                return View();
            }

            var model = new TransferRequestDetailsVM
            {
                Id = request.Id,
                AvizNumber = request.AvizNumber,
                Truck = $"{request.Truck.PlateNumber} {request.Truck.PlateLetter}",
                Driver = request.Driver.FullName,
                Department = request.Department.Name,
                PermitType = request.PermitType.Name,
                PermitNumber = request.PermitNumber,
                RequestStatus = request.RequestStatus.Name
            };

            return View(model);
        }

        // Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                return View();
            }

            _repository.Remove(request);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Ensure Order 
        public IActionResult EnsureOrder(int id)
        {
            return RedirectToAction("Recript", "Driver");
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
        #endregion
    }
}