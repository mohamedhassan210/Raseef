
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
