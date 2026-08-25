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
        /// <summary>
        /// عرض قائمة الموردين المسجلين في النظام
        /// Displays list of all registered suppliers
        /// </summary>
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

        /// <summary>
        /// عرض تفاصيل المورد والطلبات المرتبطة به
        /// Displays supplier details and associated request statistics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierRepository.GetSupplierWithDetailsAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(new SupplierDetailsVM());
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

        /// <summary>
        /// صفحة إضافة مورد جديد (GET)
        /// Displays supplier creation form
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateSupplierVM());
        }

        /// <summary>
        /// معالجة إضافة مورد جديد ورفع الشعار (POST)
        /// Handles supplier creation with optional logo upload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            catch (Exception)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ بيانات المورد، يرجى المحاولة لاحقاً.");
                return View(model);
            }
        }

        /// <summary>
        /// صفحة تعديل بيانات المورد (GET)
        /// Displays edit supplier view
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(new UpdateSupplierVM());
            }

            var updateVM = new UpdateSupplierVM
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                SupCode = supplier.SupCode,
                ExistingLogoURL = supplier.LogoURL
            };

            return View(updateVM);
        }

        /// <summary>
        /// حفظ تعديلات بيانات المورد وتحديث الشعار إن وجد (POST)
        /// Updates supplier details and logo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSupplierVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var supplier = await _supplierRepository.GetByIdAsync(model.Id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return View(model);
            }

            try
            {
                supplier.Name = model.Name;
                supplier.Phone = model.Phone;
                supplier.SupCode = model.SupCode;

                if (model.LogoFile != null)
                {
                    if (!string.IsNullOrEmpty(supplier.LogoURL))
                    {
                        _fileService.DeleteImage(supplier.LogoURL);
                    }

                    supplier.LogoURL = await _fileService.UploadImageAsync(model.LogoFile);
                }

                _supplierRepository.Update(supplier);
                await _supplierRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح!";
                return RedirectToAction("SupplierDetails", "Administration", new { id = supplier.Id });
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", $"حدث خطأ أثناء الحفظ: {realMessage}");
                return View(model);
            }
        }

        /// <summary>
        /// حذف المورد وحذف الشعار المرتبط به (POST)
        /// Deletes supplier and associated logo file
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);

            if (supplier == null)
            {
                ModelState.AddModelError("", "هذا المورد غير موجود");
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(supplier.LogoURL))
            {
                _fileService.DeleteImage(supplier.LogoURL);
            }

            _supplierRepository.Remove(supplier);
            await _supplierRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المورد بنجاح!";
            return RedirectToAction(nameof(Index));
        }
    }
}
