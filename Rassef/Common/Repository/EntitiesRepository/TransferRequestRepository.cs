
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TransferRequestRepository : Repository<TransferRequest>, ITransferRequestRepository
    {
        public TransferRequestRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
