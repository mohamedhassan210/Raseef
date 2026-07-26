
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DriverRepository : Repository<Driver>
    {
        public DriverRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
