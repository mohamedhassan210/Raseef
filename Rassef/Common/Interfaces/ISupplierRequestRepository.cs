namespace Rassef.Common.Interfaces
{
    public interface ISupplierRequestRepository : IRepository<SupplierRequest>
    {
        Task<IEnumerable<SupplierRequest>> GetAllWithDetailsAsync();
        Task<SupplierRequest?> GetByIdWithDetailsAsync(int id);
    }
}
