using Microsoft.AspNetCore.Mvc;
using Rassef.ViewModels.Driver;

namespace Rassef.Controllers
{
    public class DriverController : Controller
    {
        private readonly DriverRepository _repository;
        public DriverController(DriverRepository driverRepository)
        {
            _repository = driverRepository;
        }
        public async Task<IActionResult> Index()
        {
            var driver = await _repository.GetAllAsync();

            var driverList = new DriverListVM
            {



            };

            return View();
        }




    }
}
