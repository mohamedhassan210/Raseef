namespace Rassef.Common.Interfaces
{
    public interface IDockRepository : IRepository<Dock>
    {
        /// <summary>
        /// Feature — dock capacity/maintenance: one row per active (non-deleted)
        /// dock, carrying DepartmentId/IsUnderMaintenance/MaxTruckCount plus live
        /// occupancy — everything the Dock admin Index and the Supplier/Transfer
        /// Request dock picker need, without a second round trip per caller.
        /// Optionally scoped to a single warehouse (null = every warehouse).
        /// </summary>
        Task<IReadOnlyList<Rassef.ViewModels.Dock.DockOptionVM>> GetDockOptionsAsync(int? warehouseId = null);
    }
}