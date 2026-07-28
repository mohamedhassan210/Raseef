namespace Rassef.Controllers
{
    public class TruckController : Controller
    {
        private readonly ITruckRepository _truckRepository;
        public TruckController(ITruckRepository truckRepository)
        => _truckRepository = truckRepository;
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trucksrepo = await _truckRepository.GetAllAsync();
            var trucks = trucksrepo.Select(x => new TruckListVM
            {
                Id = x.Id,
                IsRefrigerated = x.IsRefrigerated,
                PlateLetter = x.PlateLetter,
                PlateNumber = x.PlateNumber,
                StorageCapacity = x.StorageCapacity,
                TruckTypeName = x.TruckType.Name

            });
            return View(trucks);
        }
        [HttpGet]
        public async Task<IActionResult> GetById()
        {
            return View();
        }

    }
}
