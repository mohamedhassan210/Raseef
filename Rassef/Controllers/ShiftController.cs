using Microsoft.AspNetCore.Mvc;
using Rassef.Common.Interfaces;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.Shift;

using Rassef.Filters;

namespace Rassef.Controllers
{
    [PermissionAuthorize]
    public class ShiftController : Controller
    {
        private readonly IRepository<Shift> _shiftRepository;
        private readonly IRepository<QueueSettings> _queueSettingsRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public ShiftController(
            IRepository<Shift> shiftRepository,
            IRepository<QueueSettings> queueSettingsRepository,
            IDepartmentRepository departmentRepository)
        {
            _shiftRepository = shiftRepository;
            _queueSettingsRepository = queueSettingsRepository;
            _departmentRepository = departmentRepository;
        }

        // GET: /Shift/Index (صفحة إدارة الورديات وإعدادات تصفير الأدوار)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var shifts = await _shiftRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();
            var settingsList = await _queueSettingsRepository.GetAllAsync();
            var settings = settingsList.FirstOrDefault();

            var nowTime = DateTimeOffset.Now.TimeOfDay;

            var shiftItems = shifts.Select(s =>
            {
                var start = s.StartTime;
                var end = start.Add(s.Duration);
                bool isActive;
                if (end.TotalHours <= 24)
                {
                    isActive = nowTime >= start && nowTime < end;
                }
                else
                {
                    var endNextDay = end.Subtract(TimeSpan.FromHours(24));
                    isActive = nowTime >= start || nowTime < endNextDay;
                }

                return new ShiftItemDetailVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartTime = s.StartTime,
                    Duration = s.Duration,
                    LastResetAt = s.LastResetAt,
                    IsActiveNow = isActive
                };
            }).OrderBy(x => x.StartTime).ToList();

            var deptItems = departments.Select(d => new DepartmentResetVM
            {
                Id = d.Id,
                Name = d.Name,
                Prefix = !string.IsNullOrWhiteSpace(d.Prefix)
                    ? d.Prefix.Trim().ToUpper()
                    : ((char)('A' + ((Math.Max(1, d.Id) - 1) % 26))).ToString(),
                LastResetAt = d.LastResetAt
            }).OrderBy(d => d.Name).ToList();

            var model = new ShiftManagementIndexVM
            {
                CurrentResetType = settings?.ResetType ?? ResetType.Daily,
                CurrentActiveShiftId = settings?.ShiftId,
                LastGlobalResetAt = settings?.LastGlobalResetAt,
                Shifts = shiftItems,
                Departments = deptItems
            };

            return View(model);
        }

        // POST: /Shift/SaveSettings (حفظ وتعديل استراتيجية التصفير)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(ResetType resetType, int? shiftId)
        {
            var settingsList = await _queueSettingsRepository.GetAllAsync();
            var settings = settingsList.FirstOrDefault();

            if (settings == null)
            {
                settings = new QueueSettings
                {
                    ResetType = resetType,
                    ShiftId = (resetType == ResetType.ByShift) ? shiftId : null
                };
                await _queueSettingsRepository.AddAsync(settings);
            }
            else
            {
                settings.ResetType = resetType;
                settings.ShiftId = (resetType == ResetType.ByShift) ? shiftId : null;
                _queueSettingsRepository.Update(settings);
            }

            await _queueSettingsRepository.SaveChangesAsync();
            TempData["Success"] = "تم تحديث نظام تصفير الأدوار بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/Create (إضافة وردية جديدة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, TimeSpan startTime, double durationHours)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "يرجى كتابة اسم الوردية.";
                return RedirectToAction(nameof(Index));
            }

            if (await _shiftRepository.ExistsAsync(x => x.Name.ToLower() == name.Trim().ToLower()))
            {
                TempData["Error"] = "اسم الوردية مسجل مسبقاً.";
                return RedirectToAction(nameof(Index));
            }

            var shift = new Shift
            {
                Name = name.Trim(),
                StartTime = startTime,
                Duration = TimeSpan.FromHours(durationHours > 0 ? durationHours : 8)
            };

            await _shiftRepository.AddAsync(shift);
            await _shiftRepository.SaveChangesAsync();

            TempData["Success"] = $"تمت إضافة وردية '{shift.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/Update (تعديل وردية)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string name, TimeSpan startTime, double durationHours)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                TempData["Error"] = "الوردية المطلوبة غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

            if (await _shiftRepository.ExistsAsync(x => x.Name.ToLower() == name.Trim().ToLower() && x.Id != id))
            {
                TempData["Error"] = "اسم الوردية مستخدم لوردية أخرى.";
                return RedirectToAction(nameof(Index));
            }

            shift.Name = name.Trim();
            shift.StartTime = startTime;
            shift.Duration = TimeSpan.FromHours(durationHours > 0 ? durationHours : 8);

            _shiftRepository.Update(shift);
            await _shiftRepository.SaveChangesAsync();

            TempData["Success"] = $"تم تحديث وردية '{shift.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/Delete (حذف وردية)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                TempData["Error"] = "الوردية المطلوبة غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

            _shiftRepository.Remove(shift);
            await _shiftRepository.SaveChangesAsync();

            TempData["Success"] = $"تم حذف وردية '{shift.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/ResetAll (تصفير شامل لجميع العدادات)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAll()
        {
            var settingsList = await _queueSettingsRepository.GetAllAsync();
            var settings = settingsList.FirstOrDefault();

            if (settings == null)
            {
                settings = new QueueSettings
                {
                    ResetType = ResetType.Daily,
                    LastGlobalResetAt = DateTimeOffset.Now
                };
                await _queueSettingsRepository.AddAsync(settings);
            }
            else
            {
                settings.LastGlobalResetAt = DateTimeOffset.Now;
                _queueSettingsRepository.Update(settings);
            }

            await _queueSettingsRepository.SaveChangesAsync();
            TempData["Success"] = "تم تصفير جميع عدادات الطابور لتبدأ من رقم 1 فوراً لكل الأقسام.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/ResetShift (تصفير عداد وردية محددة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetShift(int id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                TempData["Error"] = "الوردية غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

            shift.LastResetAt = DateTimeOffset.Now;
            _shiftRepository.Update(shift);
            await _shiftRepository.SaveChangesAsync();

            TempData["Success"] = $"تم تصفير عداد وردية '{shift.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Shift/ResetDepartment (تصفير عداد قسم محدد)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDepartment(int id)
        {
            var dept = await _departmentRepository.GetByIdAsync(id);
            if (dept == null)
            {
                TempData["Error"] = "القسم غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            dept.LastResetAt = DateTimeOffset.Now;
            _departmentRepository.Update(dept);
            await _departmentRepository.SaveChangesAsync();

            TempData["Success"] = $"تم تصفير عداد قسم '{dept.Name}' (الرمز: {dept.Prefix}) ليبدأ من رقم 1 فوراً.";
            return RedirectToAction(nameof(Index));
        }
    }
}
