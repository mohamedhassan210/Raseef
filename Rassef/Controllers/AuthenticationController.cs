using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Rassef.Common.Interfaces.Services.AuthenticationServices;

namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        public AuthenticationController(IUserRepository userRepository, IJwtService jwtService)
        { _userRepository = userRepository; _jwtService = jwtService; }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("home", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            return View();
        }
        [HttpGet]
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
                ModelState.AddModelError(nameof(register.UserName), "Username already exists.");
                return View(register);
            }

            if (await _userRepository.ExistsAsync(x => x.Email == Email.Create(register.Email)))
            {
                ModelState.AddModelError(nameof(register.Email), "Email already exists.");
                return View(register);
            }

            var user = new User
            {
                Name = register.FullName,
                UserName = register.UserName,
                Email = Email.Create(register.Email),
                Password = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["Success"] = "Registration completed successfully.";

            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public async Task<IActionResult> ForgetPassword()
        => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel register)
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            return View();
        }


        //Helpers
        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
        [HttpGet]
        private RedirectToActionResult ForceLogoutAndRedirect()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
