namespace Rassef.Common.Repository.EntitiesRepository
{
    public class TruckRepository : Repository<Truck>, ITruckRepository
    {
        private readonly ApplicationDbContext _context;

        public TruckRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<IEnumerable<Truck>> GetTruckWithTypeName()
        {
            return await _context.Trucks.Include(x => x.TruckType).ToListAsync();
            
        }
    }
}
