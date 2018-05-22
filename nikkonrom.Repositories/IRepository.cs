using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nikkonrom.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object entityId);

        Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task<IQueryable<TEntity>> Where(Expression<Func<TEntity, bool>> predicate);

        Task<IQueryable<TEntity>> GetAllAsync();

        void Create(TEntity entity);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}