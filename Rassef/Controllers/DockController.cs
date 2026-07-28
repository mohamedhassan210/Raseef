using Microsoft.EntityFrameworkCore;
using Rassef.ViewModels.Department;
using Rassef.ViewModels.Dock;

namespace Rassef.Controllers
{
    public class DockController  : Controller
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
        public async Task<IActionResult> Index()
        {
            var docks = await _repository.GetAllAsync();

            var dock = docks.Select(x => new DockListVM
            {
                Id = x.Id,
                DockName = x.DockName,
                WarehouseName = x.Warehouse.Name,
                DepartmentName = x.Department.Name,
                DockStatusName = x.DockStatus.Name
            });

            return View(dock);
        }

        // Get Dock By Id
        public async Task<IActionResult> Details(Guid id)
        {
            var dock = await _repository.GetByIdAsync(id);

            if (dock == null)
                return NotFound();

            var model = new DockDetailsVM
            {
                Id = id,
                DockName = dock.DockName,
                WarehouseName = dock.Warehouse.Name,
                DockStatusName = dock.DockStatus.Name,
                DepartmentName = dock.Department.Name,
                CreatedBy = dock.CreatedById
            };

            return View(model);
        }

        // Create Get
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await PopulateDropdownsForCreateAsync();
            return View(vm);
        }

        // Create Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDockVM create)
        {
            if (!ModelState.IsValid)
            {
                create = await PopulateDropdownsForCreateAsync(create);
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

            vm = await PopulateDropdownsForUpdateAsync(vm);

            return View(vm);
        }

        // Update Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDockVM updateVm)
        {
            if (!ModelState.IsValid)
            {
                updateVm = await PopulateDropdownsForUpdateAsync(updateVm);
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
                return NotFound();
            }

            _repository.Remove(dock);

            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<CreateDockVM> PopulateDropdownsForCreateAsync(CreateDockVM vm = null)
        {
            vm ??= new CreateDockVM();

            var departments = await _departmentRepo.GetAllAsync();
            var warehouses = await _warehouseRepo.GetAllAsync();
            var statuses = await _statusRepo.GetAllAsync();

            vm.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            vm.Warehouses = warehouses.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name });
            vm.DockStatus = statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

            return vm;
        }

        private async Task<UpdateDockVM> PopulateDropdownsForUpdateAsync(UpdateDockVM vm)
        {
            var departments = await _departmentRepo.GetAllAsync();
            var warehouses = await _warehouseRepo.GetAllAsync();
            var statuses = await _statusRepo.GetAllAsync();

            vm.Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name });
            vm.Warehouses = warehouses.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name });
            vm.DockStatus = statuses.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

            return vm;
        }


    }
}
