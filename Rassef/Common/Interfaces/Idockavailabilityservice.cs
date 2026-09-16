namespace Rassef.Common.Interfaces
{
    /// <summary>
    /// Feature — dock capacity + maintenance status. A dock is "full" when the number of
    /// currently active truck occupations at it (a DockAssignment row for that dock whose
    /// FinishedAt is still in the future — see DockAssignmentController's own use of this
    /// same "FinishedAt > DateTimeOffset.Now" convention) has reached MaxTruckCount.
    /// </summary>
    public class DockAvailabilityInfo
    {
        public int Id { get; set; }
        public string DockName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int MaxTruckCount { get; set; }
        public int Occupancy { get; set; }
        public bool IsUnderMaintenance { get; set; }
        public bool IsFull => Occupancy >= MaxTruckCount;
    }

    public interface IDockAvailabilityService
    {
        /// <summary>Active (non-deleted) docks belonging to a single department, with computed occupancy/IsFull.</summary>
        Task<IReadOnlyList<DockAvailabilityInfo>> GetDocksForDepartmentAsync(int departmentId);

        /// <summary>
        /// Every active dock, optionally scoped to a warehouse. Used to build the whole
        /// dataset up front for a Create form (department + dock dropdowns filtered
        /// client-side) instead of a round trip per department.
        /// </summary>
        Task<IReadOnlyList<DockAvailabilityInfo>> GetAllDocksAsync(int? warehouseId = null);

        /// <summary>
        /// Server-side re-check before persisting a Supplier/Transfer request: the dock must
        /// exist, belong to departmentId, not be deleted, not be under maintenance, and not
        /// be full. Client-side disabling in the form is cosmetic only — this is the guard
        /// that actually matters.
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateDockSelectionAsync(int dockId, int departmentId);
    }
}