using Rassef.Models.Entities;

namespace Rassef.Common.Interfaces
{
    public class TicketIssueResult
    {
        public QueueTicket Ticket { get; set; } = null!;
        public int TicketId => Ticket?.Id ?? 0;
        public string TicketNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string DockName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int WaitingCount { get; set; }
    }
}
