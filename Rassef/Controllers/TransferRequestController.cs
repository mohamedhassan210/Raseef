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

        public TransferRequestController(
            ITransferRequestRepository repository,
            ITruckRepository truckRepository,
            IDriverRepository driverRepository,
            IDepartmentRepository departmentRepository,
            IPermitTypeRepository permitTypeRepository,
            IRequestStatusRepository requestStatusRepository)
        {
            _repository = repository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _departmentRepository = departmentRepository;
            _permitTypeRepository = permitTypeRepository;
            _requestStatusRepository = requestStatusRepository;
        }

        private async Task LoadDataAsync()
        {
            ViewBag.Trucks = await _truckRepository.GetAllAsync();
            ViewBag.Drivers = await _driverRepository.GetAllAsync();
            ViewBag.Departments = await _departmentRepository.GetAllAsync();
            ViewBag.PermitTypes = await _permitTypeRepository.GetAllAsync();
            ViewBag.RequestStatuses = await _requestStatusRepository.GetAllAsync();
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var requests = await _repository.GetAllWithDetailsAsync();

            var model = requests.Select(x => new TransferRequestListVM
            {
                Id = x.Id,
                AvizNumber = x.AvizNumber,
                Truck = $"{x.Truck.PlateNumber} {x.Truck.PlateLetter}",
                Driver = x.Driver.FullName,
                Department = x.Department.Name,
                RequestStatus = x.RequestStatus.Name
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
            await LoadDataAsync();
            return View();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransferRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.AvizNumber == model.AvizNumber))
            {
                ModelState.AddModelError(nameof(model.AvizNumber), "رقم الأفيز مسجل بالفعل.");
                await LoadDataAsync();
                return View(model);
            }

            var request = new TransferRequest
            {
                AvizNumber = model.AvizNumber,
                TruckId = model.TruckId,
                DriverId = model.DriverId,
                PermitTypeId = model.PermitTypeId,
                PermitNumber = model.PermitNumber,
                DepartmentId = model.DepartmentId,
                RequestStatusId = model.RequestStatusId
            };

            await _repository.AddAsync(request);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

            await LoadDataAsync();

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

            return View(model);
        }

        // Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTransferRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return View(model);
            }

            var request = await _repository.GetByIdAsync(model.Id);

            if (request == null)
            {
                ModelState.AddModelError("", "طلب النقل غير موجود.");
                await LoadDataAsync();
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.AvizNumber == model.AvizNumber && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.AvizNumber), "رقم الأفيز مسجل بالفعل.");
                await LoadDataAsync();
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
    }
}
