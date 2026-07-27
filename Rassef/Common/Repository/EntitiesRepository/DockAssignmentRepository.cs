
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DockAssignmentRepository : Repository<DockAssignment>,IDockAssignmentRepository
    {
        public DockAssignmentRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
