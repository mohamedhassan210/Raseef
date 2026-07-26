
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DockAssignmentRepository : Repository<DockAssignment>
    {
        public DockAssignmentRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
