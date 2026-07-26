
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TransferRequestRepository : Repository<TransferRequest>
    {
        public TransferRequestRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
