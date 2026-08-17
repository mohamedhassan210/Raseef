namespace Rassef.Common.Interfaces
{
    public interface IDockAssignmentRepository : IRepository<DockAssignment>
    {
        Task<IReadOnlyList<DockAssignment>> GetAllWithDetailsAsync();
    }
}
