

namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TransferRequestRepository : Repository<TransferRequest>, ITransferRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public TransferRequestRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<IEnumerable<TransferRequest>> GetAllWithDetailsAsync()
        {
            return await _context.TransferRequests
                .Include(x => x.Truck)
                .Include(x => x.Driver)
                .Include(x => x.Department)
                .Include(x => x.PermitType)
                .Include(x => x.RequestStatus)
                .Include(x => x.CreatedBy)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.TicketStatus)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.CreatedBy)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.DockAssignments)
                        .ThenInclude(da => da.Dock)
                .ToListAsync();
        }

        public async Task<TransferRequest?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.TransferRequests
                .Include(x => x.Truck)
                .Include(x => x.Driver)
                .Include(x => x.Department)
                .Include(x => x.PermitType)
                .Include(x => x.RequestStatus)
                .Include(x => x.CreatedBy)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.TicketStatus)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.CreatedBy)
                .Include(x => x.QueueTickets)
                    .ThenInclude(q => q.DockAssignments)
                        .ThenInclude(da => da.Dock)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
