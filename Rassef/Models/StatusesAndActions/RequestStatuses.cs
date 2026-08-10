namespace Rassef.Models.StatusesAndActions
{
    public class RequestStatuses : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<TransferRequest> TransferRequests { get; set; } = new HashSet<TransferRequest>();
        public ICollection<SupplierRequest> SupplierRequests { get; set; } = new HashSet<SupplierRequest>();
    }
}
