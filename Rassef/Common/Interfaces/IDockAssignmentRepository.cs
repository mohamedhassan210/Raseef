namespace Rassef.Common.Interfaces
{
    public interface IDockAssignmentRepository : IRepository<DockAssignment>
    {
        Task<IReadOnlyList<DockAssignment>> GetAllWithDetailsAsync();

        /// <summary>
        /// Feature — dock capacity/maintenance: counts, per dock, how many
        /// DockAssignment rows are currently "active" (the truck hasn't left
        /// yet — FinishedAt is still the default/unset sentinel, mirroring the
        /// ExitTime == DateTimeOffset.MinValue "not finished" convention used
        /// for QueueTicket elsewhere in this codebase). A truck that has
        /// exited but whose assignment wasn't marked finished should NOT be
        /// counted — if that turns out to happen in practice, ExitGate/CallStation
        /// need to also stamp FinishedAt when the truck leaves the dock.
        /// </summary>
        Task<Dictionary<int, int>> GetActiveOccupancyCountsAsync(IEnumerable<int>? dockIds = null);
    }
}