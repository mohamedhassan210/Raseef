namespace Rassef.Common.Interfaces
{
    public interface ITransferRequestRepository : IRepository<TransferRequest>
    {
        Task<IEnumerable<TransferRequest>> GetAllWithDetailsAsync();

        Task<TransferRequest?> GetByIdWithDetailsAsync(int id);
    }
}
