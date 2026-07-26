
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueActionRepository : Repository<QueueAction>
    {
        public QueueActionRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
