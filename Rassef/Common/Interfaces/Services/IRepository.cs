using System.Linq.Expressions;

namespace Rassef.Common.Interfaces.Services
{
    public interface IRepository<T>
    {
        Task<T?> GetById(Guid id);
        Task<IReadOnlyList<T>> GetAll();
        Task AddAsync(T entity);
        Task<int> SaveChangesAsync();
        void Update(T entity);
        void Remove(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
