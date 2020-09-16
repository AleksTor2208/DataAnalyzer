using Castle.Windsor;
using log4net;
using log4net.Appender;
using log4net.Repository;
using Microsoft.Owin.Hosting;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace StrategyAnalyzer
{
   internal class SelfHostServiceBase : ServiceBase
   {
      private readonly string _baseUri;
      private IDisposable _webApp;
      private readonly IWindsorContainer _container;

      private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      public SelfHostServiceBase(string baseUri, IWindsorContainer container)
      {
         _baseUri = baseUri;
         _container = container;

         InitializeLog();
      }

      protected override void OnStart(string[] args)
      {
         Log.Info("Service started");
         try
         {
            _webApp = WebApp.Start(_baseUri, appBuilder => new Startup(_container).Configuration(appBuilder));
         }
         catch (Exception e)
         {
            Log.ErrorFormat("Exception thrown when tried to start the service: {0}", e.Message);
            throw;
         }
      }

      protected override void OnStop()
      {
         try
         {
            if (_webApp != null)
            {
               _webApp.Dispose();
            }
         }
         catch (Exception e)
         {
            Log.ErrorFormat("Exception thrown when tried to stop the service: {0}", e.Message);
            var currentException = e;
            while (currentException.InnerException != null)
            {
               currentException = currentException.InnerException;
               Log.ErrorFormat("Exception: {0}", e.Message);
               Log.ErrorFormat("Exception: {0}", e.StackTrace);
            }

            throw;
         }
         Log.Info("Service stopped successfully");
      }

      public static void InitializeLog()
      {
         log4net.Config.XmlConfigurator.Configure();
         var assemblyDir = Path.GetDirectoryName(
            new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

         //get the current logging repository for this application
         ILoggerRepository repository = LogManager.GetRepository();
         //get all of the appenders for the repository
         IAppender[] appenders = repository.GetAppenders();
         //only change the file path on the 'FileAppenders'
         foreach (IAppender appender in (from iAppender in appenders
                                         where iAppender is FileAppender
                                         select iAppender))
         {
            FileAppender fileAppender = appender as FileAppender;
            //set the path to your logDirectory using the original file name defined
            //in configuration
            var logFileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}-{Path.GetFileName(fileAppender.File)}";
            fileAppender.File = Path.Combine(assemblyDir, $"..\\..\\..\\log\\{logFileName}");
            //make sure to call fileAppender.ActivateOptions() to notify the logging
            //sub system that the configuration for this appender has changed.
            fileAppender.ActivateOptions();
         }
      }
   }
}