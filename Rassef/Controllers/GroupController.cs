namespace Rassef.Controllers
{
    public class GroupController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityRepository _identityRepository;
        public GroupController(IUserRepository userRepository , IIdentityRepository identityRepository)
        {
            _userRepository = userRepository;
            _identityRepository = identityRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return View();
        }

    }
}
