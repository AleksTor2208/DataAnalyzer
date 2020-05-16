﻿using log4net;
using log4net.Appender;
using log4net.Repository;
using MarketDataLoader.Converters;
using System;
using System.IO;
using System.Linq;

namespace MarketDataLoader
{
   class HtmlFilesManager
   {
      private const string HtmlExtensionFile = "*.html";
      private const string FolderKey = "MarketDataPath";
      private readonly string _folderPath;
      private readonly HtmlDocumentReader _htmlReader;
      private readonly DbConnection _dbConnection;
      private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


      public HtmlFilesManager(string[] args)
      {
         _folderPath = ValidateAndSetPath(args);
         _htmlReader = new HtmlDocumentReader();
         _dbConnection = new DbConnection();
         InitializeLog();
      }

      internal void ProcessFiles()
      {
         foreach (string htmlFile in Directory.EnumerateFiles(_folderPath, HtmlExtensionFile))
         {
            _htmlReader.SelectTablesFromFile(htmlFile);
            Log.InfoFormat("Start reading from html file. Input path is: {0}", htmlFile);

            var historicalOrders = _htmlReader.ReadHistoricalOrders();
            var bsonHistoricalOrders = BSonConverter.GenerateBSonDocuments(historicalOrders);

            var accountInfo = _htmlReader.ReadFromHtmlTable(TableType.AccountInfo);
            var paramsInfo = _htmlReader.ReadFromHtmlTable(TableType.ParamsInfo);
            var detailsInfo = _htmlReader.ReadFromHtmlTable(TableType.DetailsInfo);
            var ordersInfo = BSonConverter.GenerateOrdersInfoDocument(accountInfo, paramsInfo, detailsInfo);

            try
            {
               Log.InfoFormat("Start writing data from from {0} to mongo.", htmlFile);
               _dbConnection.LoadOrdersInfo(ordersInfo);
               _dbConnection.LoadOrders(bsonHistoricalOrders);
               Log.InfoFormat("Processing {0} file finished successfully.", htmlFile);
            }
            catch (Exception e)
            {
               Log.ErrorFormat("Error during {0} file loading. Error message: {1}", htmlFile, e.Message);
               throw;
            }
            
         }
      }

      private static string ValidateAndSetPath(string[] args)
      {
         string providedPathName = args[0].Split('=')[0];
         string providedPath = args[0].Split('=')[1];

         // check if command line is correct
         if (args.Length != 1 || providedPathName != FolderKey)
         {
            var errorMessage = "Failed to read market data path from parameter. Check the example: MarketDataPath=C:\\Users\\Documents\\sample_report\\usdjpy";
            Log.Error(errorMessage);
            throw new ArgumentException(errorMessage);
         }
         //check path
         if (!Directory.Exists(providedPath))
         {
            var errorMessage = $"Provided path \"{ providedPath}\"is not valid";
            Log.Error(errorMessage);
            throw new ArgumentException(errorMessage);
         }
         Log.InfoFormat("Input folder is: {0}", providedPath);
         return providedPath;
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
