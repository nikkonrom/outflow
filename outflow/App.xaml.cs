using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Ninject;

namespace Outflow
{
    public partial class App
    {
        public IKernel NinjectKernel { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            NinjectKernel = new StandardKernel(new OutflowNinjectModule());
            base.OnStartup(e);
        }
    }
}
