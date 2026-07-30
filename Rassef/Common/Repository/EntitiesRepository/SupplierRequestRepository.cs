namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRequestRepository : Repository<SupplierRequest>, ISupplierRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public SupplierRequestRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<IEnumerable<SupplierRequest>> GetAllWithDetailsAsync()
        {
            return await _context.SupplierRequests
                .Include(x => x.Supplier)
                .Include(x => x.Truck)
                .Include(x => x.Driver)
                .Include(x => x.Department)
                .Include(x => x.PermitType)
                .Include(x => x.CommodityType)
                .Include(x => x.RequestStatus)
                .Include(x => x.CreatedBy)
                .ToListAsync();
        }

        public async Task<SupplierRequest?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.SupplierRequests
                .Include(x => x.Supplier)
                .Include(x => x.Truck)
                .Include(x => x.Driver)
                .Include(x => x.Department)
                .Include(x => x.PermitType)
                .Include(x => x.CommodityType)
                .Include(x => x.RequestStatus)
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
