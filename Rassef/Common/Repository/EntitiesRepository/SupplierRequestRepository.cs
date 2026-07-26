
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRequestRepository : Repository<SupplierRequest>
    {
        public SupplierRequestRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
