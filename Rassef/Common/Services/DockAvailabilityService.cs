using Rassef.Common.Interfaces;

namespace Rassef.Common.Services
{
    /// <summary>
    /// Feature — dock capacity + maintenance status. Implements the contract described
    /// in IDockAvailabilityService: a dock's occupancy is the count of its DockAssignment
    /// rows whose FinishedAt is still in the future (the same "currently active" convention
    /// already used by DockAssignmentController), and it's "full" once that count reaches
    /// MaxTruckCount.
    /// </summary>
    public class DockAvailabilityService : IDockAvailabilityService
    {
        private readonly IDockRepository _dockRepository;
        private readonly IDockAssignmentRepository _dockAssignmentRepository;

        public DockAvailabilityService(
            IDockRepository dockRepository,
            IDockAssignmentRepository dockAssignmentRepository)
        {
            _dockRepository = dockRepository;
            _dockAssignmentRepository = dockAssignmentRepository;
        }

        public async Task<IReadOnlyList<DockAvailabilityInfo>> GetDocksForDepartmentAsync(int departmentId)
        {
            var all = await GetAllDocksAsync();
            return all.Where(d => d.DepartmentId == departmentId).ToList();
        }

        public async Task<IReadOnlyList<DockAvailabilityInfo>> GetAllDocksAsync(int? warehouseId = null)
        {
            var docks = await _dockRepository.GetAllAsync(query => query.Where(d => !d.IsDeleted));
            var scopedDocks = warehouseId.HasValue
                ? docks.Where(d => d.WarehouseId == warehouseId.Value).ToList()
                : docks.ToList();

            var now = DateTimeOffset.Now;
            var activeAssignments = await _dockAssignmentRepository.GetAllAsync(
                query => query.Where(a => !a.IsDeleted && a.FinishedAt > now));

            var occupancyByDock = activeAssignments
                .GroupBy(a => a.DockId)
                .ToDictionary(g => g.Key, g => g.Count());

            return scopedDocks
                .Select(d => new DockAvailabilityInfo
                {
                    Id = d.Id,
                    DockName = d.DockName,
                    DepartmentId = d.DepartmentId,
                    MaxTruckCount = d.MaxTruckCount,
                    IsUnderMaintenance = d.IsUnderMaintenance,
                    Occupancy = occupancyByDock.TryGetValue(d.Id, out var count) ? count : 0
                })
                .OrderBy(d => d.DockName)
                .ToList();
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateDockSelectionAsync(int dockId, int departmentId)
        {
            var dock = await _dockRepository.GetByIdAsync(dockId);

            if (dock == null || dock.IsDeleted)
                return (false, "الرصيف المختار غير موجود.");

            if (dock.DepartmentId != departmentId)
                return (false, "الرصيف المختار لا يتبع القسم المحدد.");

            if (dock.IsUnderMaintenance)
                return (false, "الرصيف المختار تحت الصيانة حالياً.");

            var now = DateTimeOffset.Now;
            var activeCount = (await _dockAssignmentRepository.GetAllAsync(
                query => query.Where(a => !a.IsDeleted && a.DockId == dockId && a.FinishedAt > now))).Count;

            if (activeCount >= dock.MaxTruckCount)
                return (false, "الرصيف المختار ممتلئ حالياً.");

            return (true, null);
        }
    }
}
