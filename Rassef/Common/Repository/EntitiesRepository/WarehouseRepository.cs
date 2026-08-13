namespace Rassef.Common.Repository.EntitiesRepository

{

    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository

    {

        private readonly ApplicationDbContext _db;



        public WarehouseRepository(ApplicationDbContext db) : base(db)

        {

            _db = db;

        }



        public async Task<Warehouse?> GetWithDetailsByIdAsync(int id)

        {

            return await _db.Set<Warehouse>()

                .Include(w => w.CreatedBy)

                .Include(w => w.Docks)

                .Include(w => w.Departments)

                .FirstOrDefaultAsync(w => w.Id == id);

        }



        public async Task<bool> IsNameUniqueAsync(string name)

        {

            return !await _db.Set<Warehouse>()
                .AnyAsync(w => w.Name.ToLower() == name.ToLower());

        }

    }

}