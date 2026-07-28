namespace Rassef.Common.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Supplier?> GetSupplierWithDetailsAsync(Guid id);

        Task<bool> IsNameUniqueAsync(string name, Guid? excludedId = null);

        Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludedId = null);

        Task<IEnumerable<Supplier>> GetAllSuppliersWithRequestCountAsync();

    }
}

