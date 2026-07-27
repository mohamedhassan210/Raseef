
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TruckRepository : Repository<Truck>, ITruckRepository
    {
        public TruckRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
