
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class CheckOutRepository : Repository<CheckOut>
    {
        public CheckOutRepository(ApplicationDbContext db) : base(db) { }
    }
}
