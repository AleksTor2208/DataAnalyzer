using Castle.Windsor;
using Microsoft.Owin.Hosting;
using System;
using System.ServiceProcess;

namespace StrategyAnalyzer
{
   internal class SelfHostServiceBase : ServiceBase
   {

      private readonly string _baseUri;
      private IDisposable _webApp;
      private readonly IWindsorContainer _container;

      public SelfHostServiceBase(string baseUri, IWindsorContainer container)
      {
         _baseUri = baseUri;
         _container = container;
      } 

      protected override void OnStart(string[] args)
      {
         _webApp = WebApp.Start(_baseUri, appBuilder => new Startup(_container).Configuration(appBuilder));
      }

      protected override void OnStop()
      {
         if (_webApp != null)
         {
            _webApp.Dispose();
         }
      }
   }
}