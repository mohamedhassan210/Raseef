using Rassef.Models.Identity;

namespace Rassef.Test.Controllers
{
    internal class ApplicationUser : User
    {
        public string UserName { get; set; }
    }
}