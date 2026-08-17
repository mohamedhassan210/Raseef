namespace Rassef.Common.Interfaces
{
    public interface ICheckOutRepository : IRepository<CheckOut>
    {
        Task<IReadOnlyList<CheckOut>> GetAllWithDetailsAsync();
    }
}
