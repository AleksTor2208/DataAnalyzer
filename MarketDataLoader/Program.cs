using MarketDataLoader.Converters;
using log4net;
using MarketDataLoader;
using log4net.Repository;
using log4net.Appender;
using System.Linq;
using System.IO;
using System;

namespace MarketDataLoade
{
   class Program
   {
      

      static void Main(string[] args)
      {
         var htmlFileManager = new HtmlFilesLoadManager(args);
         htmlFileManager.ProcessFiles();

         //var folderPath = @"C:\Users\ASUS\Documents\FX\sample_reports\usdjpy";
         //string inputHtml = @"C:\Users\ASUS\Documents\FX\sample_reports\usdjpy\jforex_optimizer28848197175903562.html";


         
      }

      

      public static void InitializeLog()
      {
         log4net.Config.XmlConfigurator.Configure();
         var assemblyDir = Path.GetDirectoryName(
            new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

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
            fileAppender.File = Path.Combine(assemblyDir, $"..\\..\\..\\log\\{Path.GetFileName(fileAppender.File)}");
            //make sure to call fileAppender.ActivateOptions() to notify the logging
            //sub system that the configuration for this appender has changed.
            fileAppender.ActivateOptions();
         }
      }
   }
}
