using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace StrategyAnalyzer
{
   internal class WindsorHttpDependencyResolver : IDependencyResolver
   {
      private IWindsorContainer _container;

      public WindsorHttpDependencyResolver(IWindsorContainer container)
      {
         if (container == null)
         {
            throw new ArgumentException("container can not be null");
         }
         _container = container;
      }

      public IDependencyScope BeginScope()
      {
         return new WindsorDependencyScope(_container);
      }

      public void Dispose()
      {
         if (_container != null)
         {
            _container.Dispose();
         }
      }

      public object GetService(Type serviceType)
      {
         return _container.Kernel.HasComponent(serviceType)
            ? _container.Resolve(serviceType) : null;
      }

      public IEnumerable<object> GetServices(Type serviceType)
      {
         return _container.ResolveAll(serviceType).Cast<object>().ToArray();
      }
   }
}