//namespace Rassef.Common.Repository.EntitiesRepository
//{
//    public class DockRepository : Repository<Dock>, IDockRepository
//    {
//        private readonly IDockAssignmentRepository _dockAssignmentRepository;

//        public DockRepository(ApplicationDbContext db, IDockAssignmentRepository dockAssignmentRepository) : base(db)
//        {
//            _dockAssignmentRepository = dockAssignmentRepository;
//        }

//        // Feature — dock capacity/maintenance: see IDockRepository doc. Loads every
//        // active dock (optionally scoped to one warehouse) and merges in live
//        // occupancy from DockAssignmentRepository in a single extra query, rather
//        // than the N+1 that would result from asking per-dock.
//        public async Task<IReadOnlyList<Rassef.ViewModels.Dock.DockOptionVM>> GetDockOptionsAsync(int? warehouseId = null)
//        {
//            var docks = await GetAllAsync(query => query
//                .Where(d => !d.IsDeleted)
//                .Where(d => warehouseId == null || d.WarehouseId == warehouseId.Value));

//            var occupancyByDockId = await _dockAssignmentRepository
//                .GetActiveOccupancyCountsAsync(docks.Select(d => d.Id));

//            return docks
//                .Select(d => new Rassef.ViewModels.Dock.DockOptionVM
//                {
//                    Id = d.Id,
//                    DockName = d.DockName,
//                    DepartmentId = d.DepartmentId,
//                    IsUnderMaintenance = d.IsUnderMaintenance,
//                    MaxTruckCount = d.MaxTruckCount,
//                    Occupancy = occupancyByDockId.TryGetValue(d.Id, out var occ) ? occ : 0
//                })
//                .ToList();
//        }
//    }
//}