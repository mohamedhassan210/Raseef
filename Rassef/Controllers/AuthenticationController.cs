using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {

        [HttpGet]
        public IActionResult Login()
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("home","Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel login)
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel register)
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ForgetPassword(ForgetPasswordViewModel register)
        {
            return View();
        }
        [HttpGet]
        public IActionResult UserProfile()
        {
            return View();
        }


        //Helpers
        [HttpGet]
        public IActionResult AccessDenied()
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
