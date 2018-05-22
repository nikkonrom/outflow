using System.Threading.Tasks;

namespace nikkonrom.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        Task SaveChangesAsync();
    }
}