namespace Rassef.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IFileService _fileService;

        public SupplierController(
            ISupplierRepository supplierRepository,
            IFileService fileService)
        {
            _supplierRepository = supplierRepository;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierRepository.GetAllSuppliersWithRequestCountAsync();

            var listVM = suppliers.Select(s => new SupplierListVM
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                LogoURL = s.LogoURL,
                RequestsCount = s.SupplierRequests.Count
            });

            return View(listVM);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierRepository.GetSupplierWithDetailsAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("","هذا المورد غير موجود");
                return View(supplier);
            }
                

            var detailsVM = new SupplierDetailsVM
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                LogoURL = supplier.LogoURL,
                CreatedByUserName = supplier.CreatedBy?.UserName ?? "غير محدد",
                CreatedAt = supplier.CreatedAT.LocalDateTime,
                TotalRequestsCount = supplier.SupplierRequests.Count
            };

            return View(detailsVM);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateSupplierVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSupplierVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string logoPath = string.Empty;

            if (model.LogoFile != null)
            {
                logoPath = await _fileService.UploadImageAsync(model.LogoFile);
            }

            var supplier = new Supplier
            {
                Name = model.Name,
                Phone = model.Phone,
                LogoURL = logoPath
            };

            await _supplierRepository.AddAsync(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(supplier);
            }
            

            var updateVM = new UpdateSupplierVM
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                ExistingLogoURL = supplier.LogoURL
            };

            return View(updateVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSupplierVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var supplier = await _supplierRepository.GetByIdAsync(model.Id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(supplier);
            }

            supplier.Name = model.Name;
            supplier.Phone = model.Phone;

            if (model.LogoFile != null)
            {
                _fileService.DeleteImage(supplier.LogoURL);

                supplier.LogoURL = await _fileService.UploadImageAsync(model.LogoFile);
            }

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(supplier);
            }

            _fileService.DeleteImage(supplier.LogoURL);

            _supplierRepository.Remove(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }
    }
}