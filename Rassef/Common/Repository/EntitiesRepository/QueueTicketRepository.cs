
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueTicketRepository : Repository<QueueTicket>, IQueueTicketRepository
    {
        public QueueTicketRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
