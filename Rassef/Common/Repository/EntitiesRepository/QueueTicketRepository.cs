
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueTicketRepository : Repository<QueueTicket>
    {
        public QueueTicketRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
