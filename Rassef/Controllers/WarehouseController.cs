namespace Rassef.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Dock> _dockRepository;

        public WarehousesController(
            IRepository<Warehouse> warehouseRepository,
            IRepository<Department> departmentRepository,
            IRepository<Dock> dockRepository)
        {
            _warehouseRepository = warehouseRepository;
            _departmentRepository = departmentRepository;
            _dockRepository = dockRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();

            var viewModelList = warehouses.Select(w => new WarehouseListVM
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location,
                CreatedByName = w.CreatedBy?.UserName ?? "غير محدد",
                DocksCount = w.Docks?.Count ?? 0,
                DepartmentsCount = w.Departments?.Count ?? 0
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            var viewModel = new WarehouseDetailsVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                CreatedByName = warehouse.CreatedBy?.UserName ?? "غير محدد",
                DepartmentNames = warehouse.Departments?.Select(d => d.Name).ToList() ?? new List<string>(),
                DockNames = warehouse.Docks?.Select(d => d.DockName).ToList() ?? new List<string>()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Dropdown lists 
            var viewModel = new CreateWarehouseVM
            {
                Departments = await GetDepartmentSelectListAsync(),
                Docks = await GetDockSelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWarehouseVM model)
        {
            bool nameExists = await _warehouseRepository.ExistsAsync(w => w.Name == model.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "اسم المستودع موجود بالفعل.");
            }

            if (ModelState.IsValid)
            {
                var allDepartments = await _departmentRepository.GetAllAsync();
                var selectedDepartments = allDepartments.Where(d => model.SelectedDepartmentIds.Contains(d.Id)).ToList();

                var allDocks = await _dockRepository.GetAllAsync();
                var selectedDocks = allDocks.Where(d => model.SelectedDockIds.Contains(d.Id)).ToList();



                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    Location = model.Location,
                    Departments = selectedDepartments,
                    Docks = selectedDocks
                };

                await _warehouseRepository.AddAsync(warehouse);
                await _warehouseRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Departments = await GetDepartmentSelectListAsync();
            model.Docks = await GetDockSelectListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            var viewModel = new UpdateWarehouseVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                SelectedDepartmentIds = warehouse.Departments?.Select(d => d.Id).ToList() ?? new List<Guid>(),
                Departments = await GetDepartmentSelectListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateWarehouseVM model)
        {
            if (id != model.Id) return NotFound();

            bool nameExists = await _warehouseRepository.ExistsAsync(w => w.Name == model.Name && w.Id != model.Id);
            if (nameExists)
            {
                ModelState.AddModelError("اسم", "اسم المستودع مستخدم بالفعل لمستودع آخر.");
            }

            if (ModelState.IsValid)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(model.Id);
                if (warehouse == null) return NotFound();

                var allDepartments = await _departmentRepository.GetAllAsync();
                var selectedDepartments = allDepartments.Where(d => model.SelectedDepartmentIds.Contains(d.Id)).ToList();

                warehouse.Name = model.Name;
                warehouse.Location = model.Location;
                warehouse.Departments = selectedDepartments;

                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Departments = await GetDepartmentSelectListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            var viewModel = new WarehouseListVM
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse != null)
            {
                _warehouseRepository.Remove(warehouse);
                await _warehouseRepository.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentSelectListAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetDockSelectListAsync()
        {
            var docks = await _dockRepository.GetAllAsync();
            return docks.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.DockName
            });
        }

    }
}