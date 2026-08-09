namespace Rassef.Controllers
{
    public class PositionController : Controller
    {
        private readonly IRepository<Position> _positionRepository;

        public PositionController(IRepository<Position> positionRepository)
        {
            _positionRepository = positionRepository;
        }
        [HttpGet]
        // Display all items
        public async Task<IActionResult> Index()
        {
            var positions = await _positionRepository.GetAllAsync();

            var model = positions.Select(p => new PositionViewModel
            {
                Id = p.Id,
                PositionCode = p.PositionCode,
                PositionName = p.PositionName,
                UsersCount = p.Users.Count
            });

            return View(model);
        }
        [HttpGet]
        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);

            if (position == null)
            {
                ModelState.AddModelError(string.Empty, "Position not found.");
                return View(nameof(Index), await GetPositions());
            }

            var model = new PositionDetailsViewModel
            {
                Id = position.Id,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Users = position.Users.Select(u => u.Name).ToList()
            };

            return View(model);
        }
        [HttpGet]
        // Display create page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item
        public async Task<IActionResult> Create(CreatePositionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _positionRepository.ExistsAsync(p =>
                p.PositionCode == model.PositionCode ||
                p.PositionName == model.PositionName);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Position already exists.");
                return View(model);
            }

            var position = new Position
            {
                PositionCode = model.PositionCode,
                PositionName = model.PositionName
            };

            await _positionRepository.AddAsync(position);
            await _positionRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        // Action Edit
        public async Task<IActionResult> Edit(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);

            if (position == null)
            {
                ModelState.AddModelError(string.Empty, "Position not found.");
                return View(nameof(Index), await GetPositions());
            }

            var model = new UpdatePositionViewModel
            {
                Id = position.Id,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Action Edit
        public async Task<IActionResult> Edit(UpdatePositionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var position = await _positionRepository.GetByIdAsync(model.Id);

            if (position == null)
            {
                ModelState.AddModelError(string.Empty, "Position not found.");
                return View(model);
            }

            var exists = await _positionRepository.ExistsAsync(p =>
                p.Id != model.Id &&
                (p.PositionCode == model.PositionCode ||
                 p.PositionName == model.PositionName));

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Position already exists.");
                return View(model);
            }

            position.PositionCode = model.PositionCode;
            position.PositionName = model.PositionName;

            _positionRepository.Update(position);
            await _positionRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);

            if (position == null)
            {
                ModelState.AddModelError(string.Empty, "Position not found.");
                return View(nameof(Index), await GetPositions());
            }

            var model = new PositionViewModel
            {
                Id = position.Id,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                UsersCount = position.Users.Count
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // Delete item
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);

            if (position == null)
            {
                ModelState.AddModelError(string.Empty, "Position not found.");
                return View(nameof(Index), await GetPositions());
            }

            _positionRepository.Remove(position);
            await _positionRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<PositionViewModel>> GetPositions()
        {
            var positions = await _positionRepository.GetAllAsync();

            return positions.Select(p => new PositionViewModel
            {
                Id = p.Id,
                PositionCode = p.PositionCode,
                PositionName = p.PositionName,
                UsersCount = p.Users.Count
            });
        }
    }
}
