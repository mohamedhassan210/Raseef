namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        public AuthenticationController(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthenticationController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        [HttpGet]

        // Action Intro
        public IActionResult Intro()
        {
            return View();
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
        // Action Register
        public async Task<IActionResult> Register()
        => View();

        [HttpPost]
        // just for admin
        public async Task<IActionResult> Register(RegisterViewModel register)
        {
            if (!ModelState.IsValid)
                return View(register);

            if (await _userRepository.ExistsAsync(x => x.UserName == register.UserName))
            {
                ModelState.AddModelError(nameof(register.UserName), "اسم المستخدم هذا موجود بالقعل .");
                return View(register);
            }

            if (await _userRepository.ExistsAsync(x => x.Email == Email.Create(register.Email)))
            {
                ModelState.AddModelError(nameof(register.Email), "هذا الايميل موجود بالفعل .");
                return View(register);
            }

            var user = new User
            {
                Name = register.FullName,
                UserName = register.UserName,
                Email = Email.Create(register.Email),
                HashPassword = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "تم التسجيل بنجاح.";

            return RedirectToAction(nameof(Login));
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

        // View Role
        [HttpGet]
        public async Task<IActionResult> viewRole()
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
