using Rassef.ViewModels.Authentication.UserViewModels;
using Rassef.ViewModels.Drivers;

namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IDriverRepository _driverRepository;
        private readonly IQueueTicketRepository _queueTicketRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRepository<TicketStatuses> _ticketStatusRepository;
        private readonly IRepository<SupplierRequest> _transferRequestRepository;

        public AuthenticationController(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthenticationController> logger, IDriverRepository driverRepository, IRepository<SupplierRequest> transferRequestRepository, IDepartmentRepository departmentRepository, IRepository<TicketStatuses> ticketStatusesRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _transferRequestRepository = transferRequestRepository ?? throw new ArgumentNullException(nameof(transferRequestRepository));
            _departmentRepository = departmentRepository;
            _ticketStatusRepository = ticketStatusesRepository;
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
            var h1 = BCrypt.Net.BCrypt.HashPassword("123456");
            // "$2a$11$3kET7XVlcnG0U4OXPKdIMOUTZIFkPTmbMbDX0igacQADKFLCD808O"

            var ff = BCrypt.Net.BCrypt.HashPassword("Fares0606");
            // "$2a$11$OJy53DWPgjZqx40yaSTgduDXorTU3/p3QtQ5fZHU1IvrgjMAoaQHq"

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

            var token = _jwtService.GenerateToken(user.Id, user.Email);

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
            return View();
        }



        //Helpers
        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
    }
}
