namespace Rassef.Common.Interfaces.Services
{
    public interface IRepository<T>
    {
        Task<T?> GetById(Guid id);
        Task<IReadOnlyList<T>> GetAll();
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void RemoveAsync(T entity);
    }
}
