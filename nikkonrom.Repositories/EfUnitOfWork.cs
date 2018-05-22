using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace nikkonrom.Repositories
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;

        private readonly IDictionary<Type, object> _repositories;


        public EfUnitOfWork(DbContext context)
        {
            _dbContext = context;

            _repositories = new Dictionary<Type, object>();
        }


        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories.TryGetValue(typeof(TEntity), out var repository))
            {
                return (IRepository<TEntity>)repository;
            }

            var newRepository = new EfRepository<TEntity>(_dbContext);
            _repositories.Add(typeof(TEntity), newRepository);

            return newRepository;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
