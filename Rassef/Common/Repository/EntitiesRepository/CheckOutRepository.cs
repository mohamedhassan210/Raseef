
using Rassef.Common.Interfaces;

namespace Rassef.Common.Repository.EntitiesRepository
{
    public class CheckOutRepository : Repository<CheckOut> , ICheckOutRepository
    {
        public CheckOutRepository(ApplicationDbContext db) : base(db) { }
    }
}
