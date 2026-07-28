namespace Rassef.Controllers
{
    public class DockController : Controller
    {
        private readonly IDockRepository _repository;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Warehouse> _warehouseRepo;
        private readonly IRepository<DockStatuses> _statusRepo;

        public DockController(
            IDockRepository repository,
            IRepository<Department> departmentRepo,
            IRepository<Warehouse> warehouseRepo,
            IRepository<DockStatuses> statusRepo)
        {
            _repository = repository;
            _departmentRepo = departmentRepo;
            _warehouseRepo = warehouseRepo;
            _statusRepo = statusRepo;
        }

        // Get All Docks
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var docks = await _repository.GetAllAsync();

            var dockViewModels = docks.Select(x => new DockListVM
            {
                Id = x.Id,
                DockName = x.DockName,
                WarehouseName = x.Warehouse?.Name ?? "غير محدد",
                DepartmentName = x.Department?.Name ?? "غير محدد",
                DockStatusName = x.DockStatus?.Name ?? "غير محدد"
            });

            return View(dockViewModels);
        }

        // Get Dock By Id
        [HttpGet("{id : Guid}")]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            var dock = await _repository.GetByIdAsync(id);

            if (dock == null)
                return View(dock);

            var model = new DockDetailsVM
            {
                Id = id,
                DockName = dock.DockName,
                WarehouseName = dock.Warehouse.Name ?? "غير محدد",
                DockStatusName = dock.DockStatus.Name ?? "غير محدد",
                DepartmentName = dock.Department.Name ?? "غير محدد",
                CreatedBy = dock.CreatedById
            };

            return View(model);
        }

        // Create Get
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await PopulateCreateDropdownsAsync(new CreateDockVM());
            return View(vm);
        }

        // Create Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDockVM create)
        {
            if (!ModelState.IsValid)
            {
                create = await PopulateCreateDropdownsAsync(create);
                return View(create);
            }

            var dock = new Dock
            {
                DockName = create.DockName,
                DepartmentId = create.DepartmentId,
                WarehouseId = create.WarehouseId,
                DockStatusId = create.DockStatusId
            };

            await _repository.AddAsync(dock);

            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Update Get
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var dock = await _repository.GetByIdAsync(id);

            if (dock == null)
            {
                return NotFound();
            }

            var vm = new UpdateDockVM
            {
                Id = dock.Id,
                DockName = dock.DockName,
                DepartmentId = dock.DepartmentId,
                WarehouseId = dock.WarehouseId,
                DockStatusId = dock.DockStatusId
            };

            vm = await PopulateUpdateDropdownsAsync(vm);

            return View(vm);
        }

        // Update Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDockVM updateVm)
        {
            if (!ModelState.IsValid)
            {
                updateVm = await PopulateUpdateDropdownsAsync(updateVm);
                return View(updateVm);
            }

            var dock = await _repository.GetByIdAsync(updateVm.Id);

            if (dock == null)
            {
                return NotFound();
            }

            dock.DockName = updateVm.DockName;
            dock.DepartmentId = updateVm.DepartmentId;
            dock.WarehouseId = updateVm.WarehouseId;
            dock.DockStatusId = updateVm.DockStatusId;

            _repository.Update(dock);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Delete Get
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dock = await _repository.GetByIdAsync(id);

            if (dock == null)
            {
                return NotFound();
            }

            var vm = new DockDetailsVM
            {
                Id = dock.Id,
                DockName = dock.DockName
            };

            return View(vm);
        }

        // Delete Confirm 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var dock = await _repository.GetByIdAsync(id);

            if (dock == null)
            {
                return View(dock);
            }

            _repository.Remove(dock);

            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        // 1. دالة مركزية لجلب البيانات من قاعدة البيانات (DRY)
        private async Task<(IEnumerable<SelectListItem> Depts, IEnumerable<SelectListItem> Warehouses, IEnumerable<SelectListItem> Statuses)> GetDropdownDataAsync()
        {
            var departments = await _departmentRepo.GetAllAsync();
            var warehouses = await _warehouseRepo.GetAllAsync();
            var statuses = await _statusRepo.GetAllAsync();

            return (
                departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }),
                warehouses.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name }),
                statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            );
        }

        // 2. تعبئة CreateVM
        private async Task<CreateDockVM> PopulateCreateDropdownsAsync(CreateDockVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Departments = data.Depts;
            vm.Warehouses = data.Warehouses;
            vm.DockStatus = data.Statuses;
            return vm;
        }

        // 3. تعبئة UpdateVM
        private async Task<UpdateDockVM> PopulateUpdateDropdownsAsync(UpdateDockVM vm)
        {
            var data = await GetDropdownDataAsync();
            vm.Departments = data.Depts;
            vm.Warehouses = data.Warehouses;
            vm.DockStatus = data.Statuses;
            return vm;
        }

        #endregion


    }
}
