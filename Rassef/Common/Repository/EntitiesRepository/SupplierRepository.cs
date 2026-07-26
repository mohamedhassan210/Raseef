
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRepository : Repository<Supplier>
    {
        public SupplierRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
