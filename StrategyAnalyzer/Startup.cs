using System;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Castle.Windsor;
using Owin;

namespace StrategyAnalyzer
{
   internal class Startup
   {
      private IWindsorContainer _container;

      public Startup(IWindsorContainer container)
      {
         _container = container;
      }

      internal void Configuration(IAppBuilder appBuilder)
      {
         var dependencyResolver = new WindsorHttpDependencyResolver(_container);
         appBuilder.UseWebApi(GetConfiguration(dependencyResolver));
      }

      private HttpConfiguration GetConfiguration(IDependencyResolver dependencyResolver)
      {
         var config = new HttpConfiguration();         
         config.MapHttpAttributeRoutes();
         config.DependencyResolver = dependencyResolver;
         return config;
      }
   }
}