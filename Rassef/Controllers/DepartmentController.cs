using Rassef.ViewModels.Department;

namespace Rassef.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentController(IDepartmentRepository department)
        {
            _repository = department;
        }

        // Get All Departments
        public async Task<IActionResult> Index()
        {
            var departrments = await _repository.GetAllAsync();

            var depart = departrments.Select(x => new DepartmentListVM
            {
                Id = x.Id,
                Name = x.Name,
                WarehouseName = x.Warehouse.Name
            }).ToList();

            return View(depart);
        }

        // Get Department By Id
        public async Task<IActionResult> Details(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
                return NotFound();

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseId = department.WarehouseId,
                WarehouseName = department.Warehouse.Name
            };

            return View(model);
        }

        // Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentVM create)
        {
            if (!ModelState.IsValid)
                return View(create);

            if (await _repository.ExistsAsync(x => x.Name == create.Name))
            {
                ModelState.AddModelError(nameof(create.Name), "اسم القسم مسجل بالفعل.");
                return View(create);
            }

            var department = new Department
            {
                Name = create.Name,
                WarehouseId = create.WarehouseId
            };

            await _repository.AddAsync(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Update (GET)
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
                return NotFound();

            var model = new UpdateDepartmentVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseId = department.WarehouseId
            };

            return View(model);
        }

        // Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDepartmentVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var department = await _repository.GetByIdAsync(model.Id);

            if (department == null)
                return NotFound();

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم القسم مسجل بالفعل.");
                return View(model);
            }

            department.Name = model.Name;
            department.WarehouseId = model.WarehouseId;

            _repository.Update(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Delete (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
                return NotFound();

            var model = new DepartmentDetailsVM
            {
                Id = department.Id,
                Name = department.Name,
                WarehouseName = department.Warehouse.Name
            };

            return View(model);
        }

        // Delete (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var department = await _repository.GetByIdAsync(id);

            if (department == null)
                return NotFound();

            _repository.Remove(department);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}