

namespace Rassef.Controllers
{
    public class DepartmentTypeController : Controller
    {
        private readonly IRepository<DepartmentType> _repository;

        public DepartmentTypeController(IRepository<DepartmentType> repository)
        {
            _repository = repository;
        }

        // GET: DepartmentType
        public async Task<IActionResult> Index()
        {
            var departmentTypes = await _repository.GetAllAsync();

            var model = departmentTypes.Select(x => new DepartmentTypesListVM
            {
                Id = x.Id,
                Name = x.Name,
                DepartmentsCount = x.Departments?.Count ?? 0
            }).ToList();

            return View(model);
        }

        // GET: DepartmentType/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var departmentType = await _repository.GetByIdAsync(id);

            if (departmentType == null)
            {
                return NotFound();
            }

            var model = new DepartmentTypesDetailsVM
            {
                Id = departmentType.Id,
                Name = departmentType.Name,
                DepartmentsCount = departmentType.Departments?.Count ?? 0
            };

            return View(model);
        }

        // GET: DepartmentType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DepartmentType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع القسم مسجل بالفعل.");
                return View(model);
            }

            var departmentType = new DepartmentType
            {
                Name = model.Name
            };

            await _repository.AddAsync(departmentType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DepartmentType/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var departmentType = await _repository.GetByIdAsync(id);

            if (departmentType == null)
            {
                return NotFound();
            }

            var model = new UpdateDepartmentTypesVM
            {
                Id = departmentType.Id,
                Name = departmentType.Name
            };

            return View(model);
        }

        // POST: DepartmentType/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDepartmentTypesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var departmentType = await _repository.GetByIdAsync(model.Id);

            if (departmentType == null)
            {
                return NotFound();
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم نوع القسم مسجل بالفعل.");
                return View(model);
            }

            departmentType.Name = model.Name;

            _repository.Update(departmentType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DepartmentType/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var departmentType = await _repository.GetByIdAsync(id);

            if (departmentType == null)
            {
                return NotFound();
            }

            var model = new DepartmentTypesDetailsVM
            {
                Id = departmentType.Id,
                Name = departmentType.Name,
                DepartmentsCount = departmentType.Departments?.Count ?? 0
            };

            return View(model);
        }

        // POST: DepartmentType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var departmentType = await _repository.GetByIdAsync(id);

            if (departmentType == null)
            {
                return NotFound();
            }

            _repository.Remove(departmentType);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}