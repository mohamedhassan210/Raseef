
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DockRepository : Repository<Dock>
    {
        public DockRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
