namespace Rassef.Common.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<IEnumerable<Department>> GetAllWithWareHouseName();
    }
}
