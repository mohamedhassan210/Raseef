namespace Rassef.Models.StatusesAndActions
{
    public class CommodityTypes : BaseEntity
    {
        public string Name { get; protected set; } = string.Empty;
        public ICollection<SupplierRequest> SupplierRequests = new HashSet<SupplierRequest>();

    }
}
