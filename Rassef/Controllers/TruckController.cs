namespace Rassef.Controllers
{
    public class TruckController : Controller
    {
        private readonly ITruckRepository _truckRepository;
        public TruckController(ITruckRepository truckRepository)
        =>  _truckRepository = truckRepository;
        [HttpGet]
        public IActionResult GetAll()
        {
            return View();
        }
    }
}
