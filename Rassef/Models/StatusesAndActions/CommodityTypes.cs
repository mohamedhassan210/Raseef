namespace Rassef.Models.StatusesAndActions
{
    public class CommodityTypes : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<SupplierRequest> SupplierRequests { get; set; } = new HashSet<SupplierRequest>();
    }
}
