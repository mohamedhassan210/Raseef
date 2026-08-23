using Rassef.ViewModels.Authentication.UserViewModels;
using Rassef.ViewModels.Drivers;

namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IDriverRepository _driverRepository;
        private readonly IRepository<SupplierRequest> _transferRequestRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<QueueTicket> _ticketRepository;
        private readonly ITicketEngineService _ticketEngineService;

        public AuthenticationController(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<AuthenticationController> logger,
            IDriverRepository driverRepository,
            IRepository<SupplierRequest> transferRequestRepository,
            IDepartmentRepository departmentRepository,
            IRepository<TicketStatuses> ticketStatusesRepository,
            IRepository<QueueTicket> ticketRepository,
            ITicketEngineService ticketEngineService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _transferRequestRepository = transferRequestRepository ?? throw new ArgumentNullException(nameof(transferRequestRepository));
            _departmentRepository = departmentRepository;
            _ticketStatusRepository = ticketStatusesRepository;
            _ticketRepository = ticketRepository;
            _ticketEngineService = ticketEngineService;
        }

        /// <summary>
        /// صفحة البداية والمقدمة للتعريف بالنظام
        /// Introduction and landing page for the application
        /// </summary>
        [HttpGet]
        public IActionResult Intro()
        {
            return View();
        }

        /// <summary>
        /// عرض قائمة الموظفين والمستخدمين المسجلين في النظام
        /// Lists all registered employees and users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();

            var allUsers = users.Select(u => new UserList
            {
                Name = u.Name,
                Phone = u.Phone,
                Email = u.Email != null ? u.Email.ToString() : string.Empty,
                NationalId = u.NationalId,
                UserCode = u.UserCode
            }).ToList();

            if (!allUsers.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "لا يوجد أي مستخدمين حتى الآن"
                );
            }

            return View(allUsers);
        }

        /// <summary>
        /// عرض الملف الشخصي وسجل زيارات السائق
        /// Displays driver profile and historical visit count
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("السائق", "رقم السائق مفقود.");
                return View(new DriverProfileVM());
            }

            var driver = await _driverRepository.GetByIdWithDetailsAsync(id.Value);

            if (driver == null)
            {
                ModelState.AddModelError("السائق", "هذا السائق غير موجود.");
                return View(new DriverProfileVM());
            }

            int visitsCount = driver.SupplierRequests?.Count ?? 0;
            string companyName = "غير محدد";

            if (visitsCount > 0 && driver.SupplierRequests != null)
            {
                var latestRequest = driver.SupplierRequests.OrderByDescending(r => r.Id).FirstOrDefault();
                companyName = latestRequest?.Supplier?.Name ?? "غير محدد";
            }

            var driverProfile = new DriverProfileVM
            {
                Id = driver.Id,
                Name = driver.FullName,
                NationalId = driver.NationalId,
                Phone = driver.Phone,
                CompanyName = companyName,
                VisitsCount = visitsCount
            };

            return View(driverProfile);
        }

        /// <summary>
        /// صفحة تسجيل الدخول (GET)
        /// Displays the login page or redirects if already authenticated
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("AddRoleOrView", "Authentication");
            }
            return View();
        }

        /// <summary>
        /// معالجة تسجيل الدخول والتحقق من كلمة المرور وإنشاء رمز JWT (POST)
        /// Authenticates user credentials, sets HTTP-only JWT cookie, and redirects
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }
            var user = await _userRepository.FindAsync(x =>
                x.UserName == login.UserNameOrEmail ||
                (x.Email != null && x.Email.Value == login.UserNameOrEmail));

            if (user is null)
            {
                ModelState.AddModelError(nameof(login.UserNameOrEmail),
                    "اسم المستخدم أو البريد الإلكتروني غير موجود.");

                return View(login);
            }

            try
            {
                if (!BCrypt.Net.BCrypt.Verify(login.Password, user.HashPassword))
                {
                    ModelState.AddModelError(nameof(login.Password), "كلمة المرور غير صحيحة.");
                    return View(login);
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                ModelState.AddModelError(nameof(login.Password), "يوجد مشكلة في حسابك، يرجى التواصل مع الإدارة.");
                return View(login);
            }

            var userNameToPass = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : (!string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.Email?.ToString());
            var token = _jwtService.GenerateToken(user.Id, user.Email!, userNameToPass);

            Response.Cookies.Append("AccessToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                IsEssential = true
            });

            // إذا كان المستخدم يسجل الدخول لأول مرة ولم يغير كلمة المرور من كوده بعد
            if (!user.IsChanged)
            {
                return RedirectToAction(nameof(ChangeInitialPassword));
            }

            return RedirectToAction("AddRoleOrView", "Authentication");
        }

        /// <summary>
        /// صفحة إجبار تغيير كلمة المرور الافتراضية لأول مرة (GET)
        /// Displays view forcing user to change their initial employee code password
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChangeInitialPassword()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // لو كان غيرها بالفعل، يوجهه مباشرة للصفحة الرئيسية
            if (user.IsChanged)
            {
                return RedirectToAction("AddRoleOrView", "Authentication");
            }

            return View(new Rassef.ViewModels.Authentication.ChangeInitialPasswordVM());
        }

        /// <summary>
        /// معالجة وحفظ كلمة المرور الجديدة للمستخدم لأول مرة (POST)
        /// Validates current code and sets permanent password on first login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeInitialPassword(Rassef.ViewModels.Authentication.ChangeInitialPasswordVM model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // التحقق من صحة كلمة المرور الحالية (كود الموظف)
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.HashPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "كلمة المرور الحالية (كود الموظف) غير صحيحة.");
                return View(model);
            }

            // التحقق من أن كلمة المرور الجديدة تختلف عن القديمة
            if (model.NewPassword.Trim() == model.CurrentPassword.Trim())
            {
                ModelState.AddModelError(nameof(model.NewPassword), "يجب اختيار كلمة مرور جديدة مختلفة عن كود الموظف القديم.");
                return View(model);
            }

            // حفظ كلمة المرور الجديدة وتحديث حالة التغيير
            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword.Trim());
            user.IsChanged = true;
            user.MarkAsUpdated();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعيين كلمة المرور الجديدة بنجاح!";
            return RedirectToAction("AddRoleOrView", "Authentication");
        }

        /// <summary>
        /// تسجيل الخروج وحذف الكوكيز
        /// Logs out the user and clears authentication cookie
        /// </summary>
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Authentication");
        }

        /// <summary>
        /// صفحة استرجاع كلمة المرور (GET)
        /// Displays password recovery page
        /// </summary>
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        /// <summary>
        /// معالجة استرجاع كلمة المرور (POST)
        /// Handles forgot password request
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgetPassword(ForgetPasswordViewModel register)
        {
            return View();
        }

        /// <summary>
        /// عرض الملف الشخصي للمستخدم الحالي
        /// Displays detailed profile for specified user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UserProfile(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "هذا الملف المستخدم غير موجود.");
                return View(new UserProfileViewModel());
            }
            var profile = new UserProfileViewModel
            {
                Email = user.Email,
                UserName = user.UserName,
                Name = user.Name,
                Phone = user.Phone,
                NationalId = user.NationalId
            };

            return View(profile);
        }

        /// <summary>
        /// شاشة التوجيه الرئيسية لاختيار: إضافة دور / متابعة الأدوار / لوحة الإدارة
        /// Gateway selection screen between registering, viewing queue, and dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddRoleOrView()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                var user = await _userRepository.GetByIdAsync(parsedId);
                if (user != null && !user.IsChanged)
                {
                    return RedirectToAction(nameof(ChangeInitialPassword));
                }
            }

            return View();
        }

        /// <summary>
        /// شاشة الاختيار بين خدمات التوريد (الموردين) والتحويل (الفروع)
        /// Selection screen between Supplier flow and Internal Transfer flow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SupOrTra()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
            {
                var user = await _userRepository.GetByIdAsync(parsedId);
                if (user != null && !user.IsChanged)
                {
                    return RedirectToAction(nameof(ChangeInitialPassword));
                }
            }

            return View();
        }

        /// <summary>
        /// شاشة عرض الأدوار الحية المباشرة (شاحنات جارية / انتظار / منتهية)
        /// Displays live queue status board with current active and waiting trucks
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewRole()
        {
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr!.Supplier)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr!.Driver)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr!.Truck)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr!.Driver)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr!.Truck)
                .Include(t => t.DockAssignments)
                    .ThenInclude(da => da.Dock)
            );

            var ticketViewModels = ticketsList.Select(t =>
            {
                var dockAssignment = t.DockAssignments?.OrderByDescending(x => x.AssignedAt).FirstOrDefault();
                bool isSupplier = t.SupplierRequestId != null || t.SupplierRequest != null;
                string reqType = isSupplier ? "توريد" : "تحويل";
                string company = isSupplier ? (t.SupplierRequest?.Supplier?.Name ?? "غير محدد") : "تحويل داخلي";

                return new QueueTicketListVM
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber ?? "A1",
                    TicketStatusName = t.TicketStatus != null ? t.TicketStatus.Name : "إنتظار",
                    DriverName = t.SupplierRequest?.Driver?.FullName ?? t.TransferRequest?.Driver?.FullName ?? "غير محدد",
                    TruckNumber = t.SupplierRequest?.Truck != null ? $"{t.SupplierRequest.Truck.PlateLetter} {t.SupplierRequest.Truck.PlateNumber}" : (t.TransferRequest?.Truck != null ? $"{t.TransferRequest.Truck.PlateLetter} {t.TransferRequest.Truck.PlateNumber}" : "غير محدد"),
                    DepartmentName = t.Department?.Name ?? "غير محدد",
                    DockName = dockAssignment?.Dock?.DockName ?? "A1",
                    RequestType = reqType,
                    CompanyName = company,
                    EntryTime = t.EntryTime != DateTimeOffset.MinValue ? t.EntryTime : t.CreatedAT
                };
            }).ToList();

            int waitingCount = 0, inProgressCount = 0, completedCount = 0;
            foreach (var t in ticketViewModels)
            {
                var s = t.TicketStatusName.Replace("إ", "ا").Trim();
                if (s.Contains("انتظار") || s.Contains("طابور") || s.Contains("معلق"))
                    waitingCount++;
                else if (s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل"))
                    inProgressCount++;
                else if (s.Contains("تم") || s.Contains("مكتمل") || s.Contains("منتهي") || s.Contains("خروج"))
                    completedCount++;
            }

            int GetStatusPriority(string statusName)
            {
                var s = statusName.Replace("إ", "ا").Trim();
                if (s.Contains("جاري") || s.Contains("تنفيذ") || s.Contains("تشغيل")) return 1;
                if (s.Contains("انتظار") || s.Contains("طابور") || s.Contains("معلق")) return 2;
                if (s.Contains("تم") || s.Contains("مكتمل") || s.Contains("منتهي") || s.Contains("خروج")) return 3;
                return 4;
            }

            var orderedViewModels = ticketViewModels
                .OrderBy(t => GetStatusPriority(t.TicketStatusName))
                .ThenByDescending(t => t.Id)
                .ToList();

            var viewModel = new QueueTicketIndexVM
            {
                Tickets = orderedViewModels,
                WaitingCount = waitingCount,
                InProgressCount = inProgressCount,
                CompletedCount = completedCount
            };

            return View("viewRole", viewModel);
        }

        /// <summary>
        /// استدعاء الدور القادم من قائمة الانتظار وتحويله إلى جاري التنفيذ
        /// Calls next waiting truck in queue and marks it as In-Progress
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CallNext(int? departmentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.CallNextTicketAsync(departmentId, currentUserId);
            return Json(result);
        }

        /// <summary>
        /// تحديث حالة الدور (إنتظار / جاري التنفيذ / مكتمل)
        /// Updates status of a queue ticket with audit tracking
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateTicketStatusDTO dto)
        {
            if (dto == null || dto.TicketId <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            {
                return Json(new { success = false, message = "بيانات غير صالحة." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var uId) ? uId : 1;

            var result = await _ticketEngineService.UpdateTicketStatusAsync(dto.TicketId, dto.Status, currentUserId);
            return Json(result);
        }

        /// <summary>
        /// صفحة رفض الوصول عند عدم وجود الصلاحيات الكافية
        /// Access Denied page when user role is unauthorized
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
