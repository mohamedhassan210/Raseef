namespace Rassef.Common.Repository.EntitiesRepository
{
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
        private readonly ApplicationDbContext _context;
        public DriverRepository(ApplicationDbContext db, ApplicationDbContext context) : base(db)
        {
            _context = context;
        }

        public async Task<Driver?> GetDriverWithCreatedByAsync(int id)
        {

            return await _context.Drivers.Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Driver?> GetDriverWithRequestsAsync(int id)
        {
            return await _context.Drivers
                .Include(d => d.TransferRequests)
                .Include(d => d.SupplierRequests)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        public async Task<IEnumerable<Driver>> GetDriversBySupplierIdAsync(int supplierId)
        {
            return await _context.Drivers
                .Where(d => d.SupplierRequests.Any(sr => sr.SupplierId == supplierId))
                .ToListAsync();
        }
        public async Task<bool> HasRequestsAsync(int driverId)
        {
            return await _context.SupplierRequests.AnyAsync(s => s.DriverId == driverId) ||
                await _context.TransferRequests.AnyAsync(t => t.DriverId == driverId);
        }
        public async Task<Driver?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Drivers
                .Include(d => d.SupplierRequests)
                    .ThenInclude(sr => sr.Supplier)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
