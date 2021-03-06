﻿using log4net;
using log4net.Appender;
using log4net.Repository;
using MarketDataLoader.Converters;
using MongoDB.Bson;
using System;
using System.IO;
using System.Linq;

namespace MarketDataLoader
{
   class HtmlFilesLoadManager
   {
      private const string HtmlExtensionFile = "*.html";
      private const string FolderKey = "MarketDataPath";
      
      private readonly string _folderPath;
      private readonly HtmlDocumentReader _htmlReader;
      private readonly DbConnection _dbConnection;
      private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


      public HtmlFilesLoadManager(string[] args)
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
            Log.InfoFormat("Start reading from html file. Input path is: {0}", htmlFile);
            _htmlReader.LoadFile(htmlFile);
            _htmlReader.SelectTablesFromFile(htmlFile);

            DateTime backtestStartDate, backtestEndDate;
            backtestStartDate = backtestEndDate = new DateTime();
            _htmlReader.SelectStartEndDate(htmlFile, ref backtestStartDate, ref backtestEndDate);

            var basicInfo = _htmlReader.ReadDetailsTables(TableType.BasicInfo);
            var paramsInfo = _htmlReader.ReadDetailsTables(TableType.ParamsInfo);
            var detailsInfo = _htmlReader.ReadDetailsTables(TableType.DetailsInfo);

            var orderLogs = _htmlReader.ReadOrderLogs();

            var historicalOrders = _htmlReader.ReadHistoricalOrders();

            var strategyName = historicalOrders.First().Comment;

            long linkNumber = _dbConnection.GetLinkNumber(strategyName).Result;

            var historicalOrdersBson = BSonConverter.GenerateHistoricalOrdersAsBSon(historicalOrders, paramsInfo, linkNumber);

            var strategyResultsConverter = new StrategyResultsDtoConverter(basicInfo, paramsInfo, detailsInfo, 
                                                                                 backtestStartDate, backtestEndDate);
            var strategyResultsAsBSon = strategyResultsConverter.Convert(historicalOrders, orderLogs, paramsInfo, linkNumber).ToBsonDocument();

            var ordersInfo = BSonConverter.GenerateOrdersInfoDocument(basicInfo, paramsInfo, detailsInfo);
            try
            {
               Log.InfoFormat("Start writing data from {0} to mongo.", htmlFile);
               _dbConnection.LoadOrdersInfo(ordersInfo);
               _dbConnection.LoadOrders(historicalOrdersBson);
               _dbConnection.LoadDataToResultsTable(strategyResultsAsBSon);
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
