using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;

namespace StrategyAnalyzer
{
   internal class WindsorDependencyScope : IDependencyScope
   {
      private IWindsorContainer _container;
      private IDisposable _scope;

      public WindsorDependencyScope(IWindsorContainer container)
      {
         if (container == null)
         {
            throw new ArgumentException("container can not be null");
         }
         _container = container;
         _scope = container.BeginScope();
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            _scope.Dispose();
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