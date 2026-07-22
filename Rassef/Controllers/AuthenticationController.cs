    using Microsoft.AspNetCore.Mvc;

namespace Rassef.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
