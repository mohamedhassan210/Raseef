namespace Rassef.Common.Interfaces

{

    public interface IWarehouseRepository : IRepository<Warehouse>

    {

        Task<Warehouse?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);



        Task<bool> IsNameUniqueAsync(string name, Guid? excludedId = null, CancellationToken cancellationToken = default);

    }

}