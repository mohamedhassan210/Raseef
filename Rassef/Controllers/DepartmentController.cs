namespace Rassef.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _repository;
        private readonly IRepository<Warehouse> _warehouseRepository;

        public DepartmentController(
            IDepartmentRepository department,
            IRepository<Warehouse> warehouseRepository)
        {
            _repository = department;
            _warehouseRepository = warehouseRepository;
        }

        // Display all items
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _repository.GetAllAsync(query => query.Include(d => d.Warehouse));

            var depart = departments.Select(x => new DepartmentListVM
            {
                Id = x.Id,
                Name = x.Name,
                WarehouseName = x.Warehouse?.Name ?? "غير محدد"
            }).ToList();

            return View(depart);
        }

        // Get Department By Id
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var departments = await _repository.GetAllAsync(query => query.Include(d => d.Warehouse));
            var department = departments.FirstOrDefault(d => d.Id == id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب غير موجود.");
                return View();
            }

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseId = department.WarehouseId,
                WarehouseName = department.Warehouse?.Name ?? "غير محدد"
            };

            return View(model);
        }

        // Create (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateDepartmentVM
            {
                Warehouses = await GetWarehouseSelectListAsync()
            };
            return View(vm);
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentVM create)
        {
            if (!ModelState.IsValid)
            {
                create.Warehouses = await GetWarehouseSelectListAsync();
                return View(create);
            }

            if (await _repository.ExistsAsync(x => x.Name == create.Name))
            {
                ModelState.AddModelError(nameof(create.Name), "اسم القسم مسجل بالفعل.");
                create.Warehouses = await GetWarehouseSelectListAsync();
                return View(create);
            }
            if (await _repository.ExistsAsync(x => x.Prefix == create.Prefix))
            {
                ModelState.AddModelError(nameof(create.Prefix), "هذا الـ Prefix مستخدم بالفعل.");
                create.Warehouses = await GetWarehouseSelectListAsync();
                return View(create);
            }
            var department = new Department
            {
                Name = create.Name,
                Prefix = create.Prefix,
                WarehouseId = create.WarehouseId
            };

            await _repository.AddAsync(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Update (GET)
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب تعديله غير موجود.");
                return View();
            }

            var model = new UpdateDepartmentVM
            {
                Id = department.Id,
                Name = department.Name,
                Prefix = department.Prefix,
                WarehouseId = department.WarehouseId,
                Warehouses = await GetWarehouseSelectListAsync()
            };

            return View(model);
        }

        // Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDepartmentVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            var department = await _repository.GetByIdAsync(model.Id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب تعديله غير موجود.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم القسم مسجل بالفعل.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }
            if (await _repository.ExistsAsync(x => x.Prefix == model.Prefix && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Prefix), "هذا الـ Prefix مستخدم بالفعل.");
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }
            department.Name = model.Name;
            department.Prefix = model.Prefix;
            department.WarehouseId = model.WarehouseId;

            _repository.Update(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        // Action Reset
        public async Task<IActionResult> Reset(int id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم غير موجود.");
                return RedirectToAction(nameof(Index));
            }

            department.LastResetAt = DateTimeOffset.Now;

            _repository.Update(department);
            await _repository.SaveChangesAsync();

            TempData["Success"] = "تم تصفير القسم.";

            return RedirectToAction(nameof(Index));
        }

        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var departments = await _repository.GetAllAsync(query => query.Include(d => d.Warehouse));
            var department = departments.FirstOrDefault(d => d.Id == id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب حذفه غير موجود.");
                return View();
            }

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseName = department.Warehouse?.Name ?? "غير محدد"
            };

            return View(model);
        }

        // Delete (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
            {
                ModelState.AddModelError("", "القسم المطلوب حذفه غير موجود.");
                return View();
            }

            _repository.Remove(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetWarehouseSelectListAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return warehouses.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name
            });
        }
    }
}
