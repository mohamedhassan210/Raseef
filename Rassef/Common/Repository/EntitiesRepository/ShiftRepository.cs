
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class ShiftRepository : Repository<Shift>, IShiftRepository
    {
        public ShiftRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
