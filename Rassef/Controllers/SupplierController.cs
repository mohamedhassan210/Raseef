

namespace Rassef.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IValidator<CreateSupplierVM> _createValidator;
        private readonly IValidator<UpdateSupplierVM> _updateValidator;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SupplierController(
            ISupplierRepository supplierRepository,
            IValidator<CreateSupplierVM> createValidator,
            IValidator<UpdateSupplierVM> updateValidator,
            IWebHostEnvironment webHostEnvironment)
        {
            _supplierRepository = supplierRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Details(Guid id)
        {
            var supplier = await _supplierRepository.GetSupplierWithDetailsAsync(id);
            if (supplier == null)
                return NotFound();

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
            var validationResult = await _createValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(model);
            }

            string logoPath = string.Empty;
            if (model.LogoFile != null)
            {
                logoPath = await UploadLogoFileAsync(model.LogoFile);
            }

            var supplier = new Supplier
            {
                Name = model.Name,
                Phone = model.Phone,
                LogoURL = logoPath,
            };

            await _supplierRepository.AddAsync(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();

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
            var validationResult = await _updateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(model);
            }

            var supplier = await _supplierRepository.GetByIdAsync(model.Id);
            if (supplier == null)
                return NotFound();

            supplier.Name = model.Name;
            supplier.Phone = model.Phone;

            if (model.LogoFile != null)
            {
                DeleteLogoFile(supplier.LogoURL);

                supplier.LogoURL = await UploadLogoFileAsync(model.LogoFile);
            }

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();

            if (!string.IsNullOrEmpty(supplier.LogoURL))
            {
                DeleteLogoFile(supplier.LogoURL);
            }

            _supplierRepository.Remove(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> UploadLogoFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "suppliers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/suppliers/" + uniqueFileName;
        }

        private void DeleteLogoFile(string? logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl)) return;

            string relativePath = logoUrl.TrimStart('/');
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

    }
}