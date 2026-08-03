namespace Rassef.Common.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Task<Driver?> GetDriverWithRequestsAsync(int id);
        Task<Driver?> GetDriverWithCreatedByAsync(int id);
        Task<bool> HasRequestsAsync(int driverId);
        Task<IEnumerable<Driver>> GetDriversBySupplierIdAsync(int supplierId);
    }
}
