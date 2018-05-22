using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nikkonrom.Repositories
{
    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _dbContext;

        private readonly DbSet<TEntity> _dbSet;


        public EfRepository(DbContext dbContext)
        {
            _dbContext = dbContext;

            _dbSet = dbContext.Set<TEntity>();
        }


        public void Create(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<TEntity> GetByIdAsync(object entityId)
        {
            return await _dbSet.FindAsync(entityId);
        }

        public async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task<IQueryable<TEntity>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.FromResult(_dbSet.Where(predicate));
        }

        public async Task<IQueryable<TEntity>> GetAllAsync()
        {
            return await Task.FromResult(_dbSet.AsQueryable());
        }
    }
}
