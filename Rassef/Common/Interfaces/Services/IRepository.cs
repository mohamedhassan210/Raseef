using System.Linq.Expressions;

namespace Rassef.Common.Interfaces.Services
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>> include);
        Task AddAsync(T entity);
        Task<int> SaveChangesAsync();
        void Update(T entity);
        void Remove(T entity);
        void SoftDelete(T entity);

        /// <summary>
        /// Genuine hard delete — physically removes the row via EF Core's
        /// DbSet.Remove, unlike Remove()/SoftDelete() above which both just
        /// flip IsDeleted. Used only by the department/doc card "X" buttons
        /// (Warehouses &amp; Department cards), which were explicitly asked
        /// to hard-delete rather than follow the app-wide soft-delete
        /// convention. All FKs onto Department/SupplierRequest are configured
        /// with DeleteBehavior.Restrict, so SaveChangesAsync() will throw a
        /// DbUpdateException if dependent rows still exist — callers must
        /// catch that and show a friendly message instead of letting it bubble up.
        /// </summary>
        void HardDelete(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
