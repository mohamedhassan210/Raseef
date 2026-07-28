namespace Rassef.Common.Repository.EntitiesRepository

{

    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository

    {

        private readonly ApplicationDbContext _db;



        public WarehouseRepository(ApplicationDbContext db) : base(db)

        {

            _db = db;

        }



        public async Task<Warehouse?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)

        {

            return await _db.Set<Warehouse>()

                .Include(w => w.CreatedBy)

                .Include(w => w.Docks)

                .Include(w => w.Departments)

                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        }



        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludedId = null, CancellationToken cancellationToken = default)

        {

            return !await _db.Set<Warehouse>()

                .AnyAsync(w => w.Name.ToLower() == name.ToLower() && (excludedId == null || w.Id != excludedId), cancellationToken);

        }

    }

}