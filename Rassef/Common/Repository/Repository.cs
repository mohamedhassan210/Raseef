namespace Rassef.Common.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>();
        }
        public async Task<T?> GetById(Guid id) => await _dbSet.FindAsync(id);
        public async Task<IReadOnlyList<T>> GetAll()
        => await _dbSet.AsNoTracking().ToListAsync();
        public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
        public void RemoveAsync(T entity)
        => _dbSet.Remove(entity);
        public void UpdateAsync(T entity)
        => _dbSet.Update(entity);
    }
}
