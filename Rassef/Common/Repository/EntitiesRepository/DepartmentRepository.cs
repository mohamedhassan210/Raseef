
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DepartmentRepository : Repository<Department>
    {
        public DepartmentRepository(ApplicationDbContext db) : base(db) { }
    }
}
