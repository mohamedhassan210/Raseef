using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rassef.Common.Interfaces.Services.AuthenticationServices;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.ValueObjects;
using Rassef.ViewModels.Administration;

namespace Rassef.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Position> _positionRepository;

        public AdministrationController(
            IUserRepository userRepository,
            IRepository<Position> positionRepository)
        {
            _userRepository = userRepository;
            _positionRepository = positionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Where(u => !u.IsDeleted).Include(u => u.Position)
            );

            var employees = users.Select(u => new EmployeeListVM
            {
                Id = u.Id,
                Name = u.Name,
                Phone = u.Phone,
                Email = u.Email?.ToString() ?? string.Empty,
                Role = u.Position?.PositionName ?? "غير محدد",
                NationalId = u.NationalId,
                UserCode = u.UserCode ?? string.Empty
            }).ToList();

            if (!employees.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "لا يوجد أي موظفين حتى الآن."
                );
            }

            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var employee = new EmployeeDetailsVM
            {
                Id = user.Id,
                Name = user.Name,
                UserCode = user.UserCode ?? string.Empty,
                Role = user.Position?.PositionName ?? "غير محدد",
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                NationalId = user.NationalId
            };

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var positions = await _positionRepository.GetAllAsync();

            ViewBag.Positions = positions;

            return View(new EmployeeCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync();
                ViewBag.Positions = positions;

                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Phone = model.Phone,
                Email = Email.Create(model.Email),
                PositionId = model.PositionId,
                NationalId = model.NationalId,
                UserCode = model.UserCode
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var positions = await _positionRepository.GetAllAsync();

            ViewBag.Positions = positions;

            var employee = new EmployeeEditVM
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                PositionId = user.PositionId,
                NationalId = user.NationalId,
                UserCode = user.UserCode ?? string.Empty
            };

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditVM model)
        {
            if (!ModelState.IsValid)
            {
                var positions = await _positionRepository.GetAllAsync();
                ViewBag.Positions = positions;

                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(model.Id);

            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return View(model);
            }

            user.Name = model.Name;
            user.Phone = model.Phone;
            user.Email = Email.Create(model.Email);
            user.PositionId = model.PositionId;
            user.NationalId = model.NationalId;
            user.UserCode = model.UserCode;
            user.MarkAsUpdated();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم تعديل بيانات الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var users = await _userRepository.GetAllAsync(query =>
                query.Include(u => u.Position)
            );

            var user = users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            var employee = new EmployeeDeleteVM
            {
                Id = user.Id,
                Name = user.Name,
                UserCode = user.UserCode ?? string.Empty,
                Role = user.Position?.PositionName ?? "غير محدد",
                Phone = user.Phone,
                Email = user.Email?.ToString() ?? string.Empty,
                NationalId = user.NationalId
            };

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "هذا الموظف غير موجود."
                );

                return RedirectToAction(nameof(Index));
            }

            user.IsDeleted = true;
            user.MarkAsUpdated();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الموظف بنجاح.";

            return RedirectToAction(nameof(Index));
        }
    }
}