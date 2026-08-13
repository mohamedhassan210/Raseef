namespace Rassef.Common.Repository.EntitiesRepository
{
    public class QueueSettingsRepository : Repository<QueueSettings>, IRepository<QueueSettings>
    {
        public QueueSettingsRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
