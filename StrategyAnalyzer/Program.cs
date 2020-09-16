using Castle.Windsor;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using StrategyAnalyzer.Windsor;
using System.Configuration;
using Microsoft.Owin.Hosting;
using System.Threading;

namespace StrategyAnalyzer
{
   static class Program
   {
      private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      static void Main(string[] args)
      {
         Log.InfoFormat("Starting StrategyAnalyzer server with commandLine arguments: \n{0}", args);
         bool isDebug; 
         bool.TryParse(ConfigurationManager.AppSettings["IsDebug"], out isDebug);
         try
         {
            using (var container = new WindsorContainer())
            {
               ComposeContainer(container, args);
               string baseUri = "http://localhost:50061";
               if (isDebug == true)
               {
                  StartServiceInDebug(baseUri, container);
               }
               else
               {
                  StartService(baseUri, container);
               }
            }
         }
         catch (Exception e)
         {
            Log.ErrorFormat("Service stopped with error: {0}, stackTrace: {1}", e.Message,
                                                                                e.StackTrace);
         }
      }

      private static void StartService(string baseUri, WindsorContainer container)
      {
         ServiceBase[] ServicesToRun;
         ServicesToRun = new ServiceBase[]
         {
              new SelfHostServiceBase(baseUri, container)
         };
         ServiceBase.Run(ServicesToRun);
      }

      private static void StartServiceInDebug(string baseUri, WindsorContainer container)
      {
         using (WebApp.Start(baseUri, appBuilder => new Startup(container).Configuration(appBuilder)))
         {
            Log.InfoFormat("Server is running at {0}", baseUri);
            Thread.Sleep(Timeout.Infinite);
         }
      }

      private static void ComposeContainer(WindsorContainer container, string[] args)
      {
         container.Install(
            new ConfigurationInstaller(args),
            new ControllerInstaller());
      }
   }
}
