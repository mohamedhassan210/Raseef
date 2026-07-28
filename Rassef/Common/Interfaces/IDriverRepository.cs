namespace Rassef.Common.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Task<Driver?> GetDriverWithRequestsAsync(Guid id);
        Task<Driver?> GetDriverWithCreatedByAsync(Guid id);
        Task<bool> HasRequestsAsync(Guid driverId);
    }
}
