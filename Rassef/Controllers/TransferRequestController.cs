using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rassef.Models.Entities;
using Rassef.ViewModels.TransferRequest;
using System.Security.Claims;

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

        private async Task LoadSelectListsAsync(CreateTransferRequestVM model)
        {
            var departments = await _departmentRepository.GetAllAsync();
            var permitTypes = await _permitTypeRepository.GetAllAsync();

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
            return View();
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
                await LoadDataAsync();
                return View(model);
            }

            // استخراج معرف المستخدم الحالي
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // تحديد الحالة الافتراضية للطلب (مثلاً 1 لـ Pending/New حسب قاعدة البيانات لديك)
            int defaultStatusId = 1;

            var request = new TransferRequest
            {
                AvizNumber = model.AvizNumber,
                PermitNumber = model.PermitNumber,
                DepartmentId = model.DepartmentId,
                PermitTypeId = model.PermitTypeId,
                RequestStatusId = defaultStatusId,
                CreatedById = userId
            };

            await _repository.AddAsync(request);
            await _repository.SaveChangesAsync();

            // بعد نجاح الإضافة ينتقل إلى صفحة إنهاء الإعداد
            return RedirectToAction("TransferDrivers", "Driver");
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
        public async Task<IActionResult> EnsureOrder(int id)
        {
            return RedirectToAction("Recript", "Driver");
        }
    }
}