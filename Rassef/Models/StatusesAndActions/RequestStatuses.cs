namespace Rassef.Models.StatusesAndActions
{
    public class RequestStatuses : BaseEntity
    {
        public string Name { get; protected set; } = string.Empty;
        public ICollection<TransferRequest> TransferRequests { get; set; } = new HashSet<TransferRequest>();
    }
}
