
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(ApplicationDbContext db) : base(db) { }
    }
}
