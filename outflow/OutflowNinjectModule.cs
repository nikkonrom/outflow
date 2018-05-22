using System.Data.Entity;
using nikkonrom.Outflow.Repositories;
using nikkonrom.Repositories;
using Ninject.Modules;

namespace Outflow
{
    public class OutflowNinjectModule : NinjectModule
    {
        public override void Load()
        {

            Bind<DbContext>().To<OutflowDbContext>();
            Bind<IUnitOfWork>().To<EfUnitOfWork>();
        }
    }
}