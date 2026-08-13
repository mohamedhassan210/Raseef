
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetSupplierWithDetailsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.CreatedBy)
                .Include(s => s.SupplierRequests)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludedId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            var query = _context.Suppliers.AsQueryable();

            if (excludedId.HasValue)
            {
                query = query.Where(s => s.Id != excludedId.Value);
            }

            bool exists = await query.AnyAsync(s => s.Name.Trim().ToLower() == name.Trim().ToLower());
            return !exists;
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludedId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            var query = _context.Suppliers.AsQueryable();

            if (excludedId.HasValue)
            {
                query = query.Where(s => s.Id != excludedId.Value);
            }

            bool exists = await query.AnyAsync(s => s.Phone.Trim() == phone.Trim());
            return !exists;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersWithRequestCountAsync()
        {
            return await _context.Suppliers
                .Include(s => s.SupplierRequests)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}