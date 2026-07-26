
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DockRepository : Repository<Dock>, IDockRepository
    {
        public DockRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
