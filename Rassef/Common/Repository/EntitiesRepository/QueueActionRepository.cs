
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueActionRepository : Repository<QueueAction>, IQueueActionRepository
    {
        public QueueActionRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
