
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class WarehouseRepository : Repository<Warehouse>
    {
        public WarehouseRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
