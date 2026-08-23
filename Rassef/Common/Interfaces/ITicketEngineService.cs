namespace Rassef.Common.Interfaces
{
    public interface ITicketEngineService
    {
        Task<(string Prefix, int Counter, string TicketNumber, Shift? ActiveShift)> GenerateTicketNumberAsync(int departmentId);
        Task<TicketIssueResult> IssueTransferTicketAsync(int departmentId, int transferRequestId, int userId);
        Task<TicketIssueResult> IssueSupplierTicketAsync(int departmentId, int supplierRequestId, int userId);
        Task<TicketIssueResult> IssueGeneralTicketAsync(int departmentId, int? supplierRequestId, int? transferRequestId, int? ticketStatusId, int userId);
        Task<TicketStatusUpdateResult> CallNextTicketAsync(int? departmentId, int userId);
        Task<TicketStatusUpdateResult> UpdateTicketStatusAsync(int ticketId, string targetStatusName, int userId);
    }
}
