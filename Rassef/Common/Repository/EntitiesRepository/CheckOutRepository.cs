namespace Rassef.Common.Repository.EntitiesRepository
{
    public class CheckOutRepository : Repository<CheckOut>, ICheckOutRepository
    {
        private readonly ApplicationDbContext _context;
        public CheckOutRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        // CQ-8: جلب CheckOuts مع كل Navigate Properties لتفادي NullReferenceException
        public async Task<IReadOnlyList<CheckOut>> GetAllWithDetailsAsync()
        {
            return await _context.CheckOuts
                .Include(c => c.QueueTicket)
                .Include(c => c.ExitType)
                .Include(c => c.CreatedBy)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
