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
            var suppliers = await _supplierRepository.GetAllAsync(
                include: query => query.Include(s => s.CreatedBy)
            );

            var model = suppliers.Select(s => new SupplierListVM
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                SupCode = s.SupCode,
                LogoURL = s.LogoURL,
                HostEmployeeName = s.CreatedBy != null ? s.CreatedBy.Name : "غير محدد"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        // Display details
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierRepository.GetSupplierWithDetailsAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
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
        // Display create page
        public IActionResult Create()
        {
            return View(new CreateSupplierVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Create new item
        public async Task<IActionResult> Create(CreateSupplierVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                string logoPath = string.Empty;

                if (model.LogoFile != null)
                {
                    logoPath = await _fileService.UploadImageAsync(model.LogoFile);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int createdById = string.IsNullOrEmpty(userIdClaim) ? 1 : int.Parse(userIdClaim);

                var supplier = new Supplier
                {
                    Name = model.Name,
                    Phone = model.Phone,
                    LogoURL = logoPath,
                    CreatedById = createdById,
                    SupCode = model.SupCode,
                };

                await _supplierRepository.AddAsync(supplier);
                await _supplierRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة المورد بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content($"السبب الحقيقي للخطأ: {realMessage}");
            }
        }

        [HttpGet]
        // Action Edit
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
                SupCode = supplier.SupCode, // <-- ربط كود المورد
                ExistingLogoURL = supplier.LogoURL
            };

            return View(updateVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSupplierVM model)
        {
            // 1. التحقق من صحة المدخلات
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. جلب المورد من قاعدة البيانات
            var supplier = await _supplierRepository.GetByIdAsync(model.Id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(model);
            }

            try
            {
                // 3. تحديث الخصائص
                supplier.Name = model.Name;
                supplier.Phone = model.Phone;
                supplier.SupCode = model.SupCode;

                // 4. معالجة الصورة
                if (model.LogoFile != null)
                {
                    if (!string.IsNullOrEmpty(supplier.LogoURL))
                    {
                        _fileService.DeleteImage(supplier.LogoURL);
                    }

                    supplier.LogoURL = await _fileService.UploadImageAsync(model.LogoFile);
                }

                // 5. حفظ التعديلات
                _supplierRepository.Update(supplier);
                await _supplierRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", $"حدث خطأ أثناء الحفظ: {realMessage}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Delete item
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
