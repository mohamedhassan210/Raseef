using Microsoft.AspNetCore.Mvc;
using Rassef.Common.Repository;
using Rassef.Models.Entities;
using Rassef.ViewModels.Shift;

namespace Rassef.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IRepository<Shift> _repository;

        public ShiftController(IRepository<Shift> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var shifts = await _repository.GetAllAsync();

            var model = shifts.Select(x => new ShiftListVM
            {
                Id = x.Id,
                Name = x.Name,
                StartDate = x.StartDate,
                Duration = x.Duration
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب غير موجود.");
                return View();
            }

            var model = new ShiftDetailsVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartDate = shift.StartDate,
                Duration = shift.Duration,
                CreatedAT = shift.CreatedAT
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShiftVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _repository.ExistsAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الشيفت مسجل بالفعل.");
                return View(model);
            }

            var shift = new Shift
            {
                Name = model.Name,
                StartDate = model.StartDate,
                Duration = model.Duration
            };

            await _repository.AddAsync(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب تعديله غير موجود.");
                return View();
            }

            var model = new UpdateShiftVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartDate = shift.StartDate,
                Duration = shift.Duration
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateShiftVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var shift = await _repository.GetByIdAsync(model.Id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب تعديله غير موجود.");
                return View(model);
            }

            if (await _repository.ExistsAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "اسم الشيفت مسجل بالفعل.");
                return View(model);
            }

            shift.Name = model.Name;
            shift.StartDate = model.StartDate;
            shift.Duration = model.Duration;

            _repository.Update(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب حذفه غير موجود.");
                return View();
            }

            var model = new ShiftDetailsVM
            {
                Id = shift.Id,
                Name = shift.Name,
                StartDate = shift.StartDate,
                Duration = shift.Duration,
                CreatedAT = shift.CreatedAT
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
            {
                ModelState.AddModelError("", "الشيفت المطلوب حذفه غير موجود.");
                return View();
            }

            _repository.Remove(shift);
            await _repository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}