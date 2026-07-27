
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
        public DriverRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
