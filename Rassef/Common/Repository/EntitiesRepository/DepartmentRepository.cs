
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DepartmentRepository : Repository<Department> , IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext db) : base(db) { }
    }
}
