namespace Rassef.Common.Interfaces

{

    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse?> GetWithDetailsByIdAsync(int id);
        Task<bool> IsNameUniqueAsync(string name);
    }

}