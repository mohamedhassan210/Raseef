
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
