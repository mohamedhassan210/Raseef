
namespace Rassef.Common.Repository.EntitiesRepository
{
    public class PositionRepository : Repository<Position>, IPositionRepository
    {
        public PositionRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
