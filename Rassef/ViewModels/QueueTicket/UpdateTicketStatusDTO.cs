namespace Rassef.ViewModels.QueueTicket
{
    public class UpdateTicketStatusDTO
    {
        public int TicketId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
    }
}
