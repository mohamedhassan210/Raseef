

namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DepartmentRepository : Repository<Department> , IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

  
        public DepartmentRepository(ApplicationDbContext db) : base(db) 
        {
            _context = db;
        }

        public async Task<IEnumerable<Department>> GetAllWithWareHouseName()
        {
            return await _context.Departments.Include(x=> x.Warehouse).ToListAsync();
        }
    }
}
