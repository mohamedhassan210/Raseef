
namespace Rassef.Common.Interfaces.Services
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(
    Func<IQueryable<T>, IQueryable<T>> include);
        Task AddAsync(T entity);
        Task<int> SaveChangesAsync();
        void Update(T entity);
        void Remove(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    }
}
