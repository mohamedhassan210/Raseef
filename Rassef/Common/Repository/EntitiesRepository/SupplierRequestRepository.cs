
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRequestRepository : Repository<SupplierRequest>, ISupplierRequestRepository
    {
        public SupplierRequestRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
