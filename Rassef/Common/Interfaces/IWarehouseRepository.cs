namespace Rassef.Common.Interfaces

{

    public interface IWarehouseRepository : IRepository<Warehouse>

    {

        Task<Warehouse?> GetWithDetailsByIdAsync(Guid id);



        Task<bool> IsNameUniqueAsync(string name);

    }

}