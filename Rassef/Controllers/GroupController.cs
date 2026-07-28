namespace Rassef.Controllers
{
    public class GroupController : Controller
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            return View();
        }
       
    }
}
