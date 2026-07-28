using Microsoft.AspNetCore.Mvc;
using Rassef.Common.Interfaces;
using Rassef.Models.Entities;

namespace Rassef.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehousesController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetWithDetailsByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location")] Warehouse warehouse)
        {
            bool isUnique = await _warehouseRepository.IsNameUniqueAsync(warehouse.Name);
            if (!isUnique)
            {
                ModelState.AddModelError("Name", "اسم المستودع موجود بالفعل.");
            }

            if (ModelState.IsValid)
            {
                await _warehouseRepository.AddAsync(warehouse);
                await _warehouseRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Location")] Warehouse warehouse)
        {
            if (id != warehouse.Id) return NotFound();

            bool isUnique = await _warehouseRepository.IsNameUniqueAsync(warehouse.Name, excludedId: id);
            if (!isUnique)
            {
                ModelState.AddModelError("Name", "اسم المستودع مستخدم بالفعل لمستودع آخر.");
            }

            if (ModelState.IsValid)
            {
                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var warehouse = await _warehouseRepository.GetByIdAsync(id.Value);
            if (warehouse == null) return NotFound();

            return View(warehouse);
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
    }
}