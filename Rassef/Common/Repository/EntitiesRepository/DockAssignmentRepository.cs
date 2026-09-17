
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DockAssignmentRepository : Repository<DockAssignment>, IDockAssignmentRepository
    {
        private readonly ApplicationDbContext _context;
        public DockAssignmentRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        // CQ-7: جلب DockAssignments مع Dock و QueueTicket لتفادي "غير محدد" في الواجهة
        public async Task<IReadOnlyList<DockAssignment>> GetAllWithDetailsAsync()
        {
            return await _context.DockAssignments
                .Include(da => da.Dock)
                .Include(da => da.QueueTicket)
                .Include(da => da.CreatedBy)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
