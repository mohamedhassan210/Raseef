
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TruckRepository : Repository<Truck>
    {
        public TruckRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
