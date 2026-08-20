namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueSettingsRepository : Repository<QueueSettings>, IQueueSettingsRepository
    {
        public QueueSettingsRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
