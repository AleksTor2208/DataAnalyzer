using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using StrategyAnalyzer.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.Windsor
{
   class ConfigurationInstaller : IWindsorInstaller
   {
      private readonly IEnumerable<string> _args;

      public ConfigurationInstaller(string[] args)
      {
         _args = args;
      }

      public void Install(IWindsorContainer container, IConfigurationStore store)
      {
         container.Register(Component.For<IDbConnection>().ImplementedBy<MongoDbConnection>());
         container.Register(Component.For<IDataProvider>().ImplementedBy<OrdersDataProvider>());
      }
   }
}
