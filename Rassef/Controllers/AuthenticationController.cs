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

        public AuthenticationController(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<AuthenticationController> logger,
            IDriverRepository driverRepository,
            IRepository<SupplierRequest> transferRequestRepository,
            IDepartmentRepository departmentRepository,
            IRepository<TicketStatuses> ticketStatusesRepository,
            IRepository<QueueTicket> ticketRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _transferRequestRepository = transferRequestRepository ?? throw new ArgumentNullException(nameof(transferRequestRepository));
            _departmentRepository = departmentRepository;
            _ticketStatusRepository = ticketStatusesRepository;
            _ticketRepository = ticketRepository;
        }

        [HttpGet]

        // Action Intro
        public IActionResult Intro()
        {
            return View();
        }
        // every employee 
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();

            var allUsers = users.Select(u => new UserList
            {
                Name = u.Name,
                Phone = u.Phone,
                Email = u.Email.ToString(),
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


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("السائق", "رقم السائق مفقود.");
                return View(new DriverProfileVM());
            }

            // هنا هننادي الدالة اللي بتجيب السائق بالطلبات والموردين اللي جواه
            var driver = await _driverRepository.GetByIdWithDetailsAsync(id.Value);

            if (driver == null)
            {
                ModelState.AddModelError("السائق", "هذا السائق غير موجود.");
                return View(new DriverProfileVM());
            }

            // 1. حساب عدد الزيارات من الـ ICollection مباشرة
            int visitsCount = driver.SupplierRequests?.Count ?? 0;

            // 2. استنتاج اسم الشركة من "أحدث طلب" في الـ ICollection
            string companyName = "غير محدد";

            if (visitsCount > 0)
            {
                // رتبناهم تنازلي وجبنا أول واحد (أحدث طلب)، وبعدين دخلنا على المورد جبنا اسمه
                companyName = driver.SupplierRequests
                    .OrderByDescending(r => r.Id)
                    .First()
                    .Supplier?.Name ?? "غير محدد";
            }

            // 3. بناء الـ ViewModel
            var driverProfile = new DriverProfileVM
            {
                Id = driver.Id,
                Name = driver.FullName,
                NationalId = driver.NationalId, // تأكد من اسم الخاصية عندك في الموديل
                Phone = driver.Phone, // تأكد من اسم الخاصية
                CompanyName = companyName,
                VisitsCount = visitsCount
            };

            return View(driverProfile);
        }

        [HttpGet]
        // Action Login
        public async Task<IActionResult> Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SupOrTra", "Authentication");
            }
            return View();
        }
        [HttpPost]
        // Action Login
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }
            var user = await _userRepository.FindAsync(x =>
                x.UserName == login.UserNameOrEmail ||
                x.Email!.Value == login.UserNameOrEmail);

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
                // في حال كانت كلمة المرور في قاعدة البيانات غير مشفرة بشكل صحيح
                ModelState.AddModelError(nameof(login.Password), "يوجد مشكلة في حسابك، يرجى التواصل مع الإدارة.");
                return View(login);
            }

            var userNameToPass = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : (!string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.Email?.ToString());
            var token = _jwtService.GenerateToken(user.Id, user.Email, userNameToPass);

            Response.Cookies.Append("AccessToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                IsEssential = true
            });
            return RedirectToAction("AddRoleOrView", "Authentication");
        }
        
    
        [HttpGet]
        // Action ForgetPassword
        public async Task<IActionResult> ForgetPassword()
        => View();

        [HttpPost]
        // Action ForgetPassword
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel register)
        {
            return View();
        }
        [HttpGet]
        // Action UserProfile
        public async Task<IActionResult> UserProfile(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError(nameof(user.UserName), "هذا الملف المستخدم غير موجود .");
                return View(user);
            }
            var profile = new UserProfileViewModel
            {
                Email = Email.Create(user.Email.Value),
                UserName = user.UserName,
                Name = user.Name,
                Phone = user.Phone,
                NationalId = user.NationalId
            };
            if (profile == null)
            {
                ModelState.AddModelError(nameof(profile.Name), "هذا الملف الشخصى غير موجود .");
                return View(profile);
            }

            return View(profile);

        }
        // add role action 
        [HttpGet]
        public async Task<IActionResult> AddRoleOrView()
        {
            return View();
        }

        // Supplier of Transfer 
        [HttpGet]
        public async Task<IActionResult> SupOrTra()
        {
            return View();
        }

        // Supplier of Transfer 
        [HttpGet]
        public async Task<IActionResult> ViewRole()
        {
            var ticketsList = await _ticketRepository.GetAllAsync(query => query
                .Include(t => t.Department)
                .Include(t => t.TicketStatus)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr.Supplier)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr.Driver)
                .Include(t => t.SupplierRequest)
                    .ThenInclude(sr => sr.Truck)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr.Driver)
                .Include(t => t.TransferRequest)
                    .ThenInclude(tr => tr.Truck)
                .Include(t => t.DockAssignments)
                    .ThenInclude(da => da.Dock)
            );

            var ticketViewModels = ticketsList.Select(t => {
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

            // حساب الإحصائيات في مرور واحد
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

            // ترتيب الأدوار منطقياً: الجارية أولاً ثم بالانتظار ثم المنتهية
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



        //Helpers
        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
    }
}
