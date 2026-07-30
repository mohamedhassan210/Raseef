namespace Rassef.Common.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Supplier?> GetSupplierWithDetailsAsync(int id);

        Task<bool> IsNameUniqueAsync(string name, int? excludedId = null);

        Task<bool> IsPhoneUniqueAsync(string phone, int? excludedId = null);

        Task<IEnumerable<Supplier>> GetAllSuppliersWithRequestCountAsync();

    }
}

