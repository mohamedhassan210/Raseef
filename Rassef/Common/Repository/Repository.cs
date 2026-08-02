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
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();
        public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
        public void Remove(T entity)
        => _dbSet.Remove(entity);
        public void Update(T entity)
        => _dbSet.Update(entity);
        public async Task<int> SaveChangesAsync()
        => await _db.SaveChangesAsync();
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);



    }
}
