using log4net;
using log4net.Appender;
using log4net.Repository;
using MarketDataLoader.Converters;
using MarketDataLoader.ExtensionMethods;
using MarketDataLoader.Model;
using ModelLayer;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
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
            var orderLogs = _htmlReader.ReadOrderLogs();

            var historicalOrders = _htmlReader.ReadHistoricalOrders();

            var currency = _htmlReader.GetCurrency();
            var strategyName = _htmlReader.GetStrategyName();//historicalOrders.First().Comment;

            long linkNumber = _dbConnection.GetLinkNumber(strategyName).Result;

            var basicInfo = _htmlReader.ReadDetailsTables(TableType.BasicInfo);
            var paramsInfo = _htmlReader.ReadDetailsTables(TableType.ParamsInfo);
            var detailsInfo = _htmlReader.ReadDetailsTables(TableType.DetailsInfo);


            SetupInfo setupInfo = PrepareSetupInfo(htmlFile, basicInfo, paramsInfo, detailsInfo, strategyName, currency);

            var calendarLogs = SetupCalendarLogs(setupInfo.StartDate, setupInfo.EndDate, historicalOrders, orderLogs);
            EnrichHistoricalOrdersWithCommissions(historicalOrders);

            // should be modified, commissions should be already calculated in Enrichment step
            var strategyResultsConverter = new StrategyResultsDtoConverter(basicInfo, paramsInfo, detailsInfo, setupInfo, calendarLogs);
            var strategyResultsAsBSon = strategyResultsConverter.Convert(historicalOrders, orderLogs, paramsInfo, linkNumber).ToBsonDocument();


            var historicalOrdersBson = BSonConverter.GenerateHistoricalOrdersAsBSon(historicalOrders, paramsInfo, linkNumber);

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

      private void EnrichHistoricalOrdersWithCommissions(List<HistoricalOrderDto> historicalOrders)
      {
         //var aaa = SetupCalendarLogs()
      }

      private SetupInfo PrepareSetupInfo(string htmlFile, Dictionary<string, string> basicInfo, Dictionary<string, string> paramsInfo, 
                                         Dictionary<string, string> detailsInfo, string strategyName, string currency)
      {
         var setupInfo = new SetupInfo()
         {
            AccountCurrency = currency,
            StrategyName = strategyName,
            InitialDeposit = basicInfo["Initial deposit"].ToDouble(),
            Commission = detailsInfo["Commission"].ToDouble(),
            ClosedPositions = detailsInfo["Closed positions"].ToInteger()
         };
         _htmlReader.SelectStartEndDate(htmlFile, setupInfo);
         return setupInfo;
      }

      private List<CalendarLog> SetupCalendarLogs(DateTime startDate, DateTime endDate, List<HistoricalOrderDto> historicalOrders, IEnumerable<OrderLog> orderLogs)
      {
         const int LastOperationDayHour = 21;
         var calendarLogs = new List<CalendarLog>();
         DateTime currentdate = startDate.AddDays(-1);
         while (DateTime.Compare(currentdate, endDate) <= 0)
         {
            //if (currentdate.DayOfWeek == DayOfWeek.Saturday || currentdate.DayOfWeek == DayOfWeek.Sunday)
            //{
            //   currentdate = currentdate.AddDays(1);
            //   continue;
            //}

            var calendarLog = new CalendarLog()
            {
               OperationDay = currentdate
            };

            var logWithCommission = orderLogs.FirstOrDefault(log => DateTime.Compare(log.OperationDay.Date, currentdate.Date) == 0);
            if (logWithCommission != null)
            {
               calendarLog.SumCmsn = logWithCommission.Comisions.Sum();
            }

            calendarLogs.Add(calendarLog);
            currentdate = currentdate.AddDays(1);
         }

         for (int i = 0; i < calendarLogs.Count(); i++)
         {
            var currentOrder = historicalOrders.FirstOrDefault(order => DateTime.Compare(
                                                               order.OpenDate.Date, calendarLogs[i].OperationDay.Date) == 0);
            if (currentOrder != null)
            {
               //TradesOpened set
               //if Open trade hour is earlier then 21 gmt then order assignes to current day
               if (currentOrder.OpenDate.Hour <= LastOperationDayHour)
               {
                  calendarLogs[i].TradesOpened.Add(currentOrder.Label);
               }
               if (currentOrder.OpenDate.Hour > LastOperationDayHour)
               {
                  calendarLogs[i + 1].TradesOpened.Add(currentOrder.Label);
               }
            }
         }

         for (int i = 0; i < calendarLogs.Count(); i++)
         {
            var currentOrder = historicalOrders.FirstOrDefault(order => DateTime.Compare(
                                                               order.CloseDate.Date, calendarLogs[i].OperationDay.Date) == 0);
            if (currentOrder != null)
            {
               //TradesClosed set
               if (currentOrder.CloseDate.Hour <= LastOperationDayHour)
               {
                  calendarLogs[i].TradesClosed.Add(currentOrder.Label);
               }
               if (currentOrder.CloseDate.Hour > LastOperationDayHour)
               {
                  calendarLogs[i + 1].TradesClosed.Add(currentOrder.Label);
               }
            }
         }
         //AvgCmsn = сумма_комиссий / (сделок_открыто + сделок_закрыто)
         for (int i = 0; i < calendarLogs.Count(); i++)
         {
            var currentLog = calendarLogs[i];
            int tradesTotal = currentLog.TradesOpened.Count() + currentLog.TradesClosed.Count();
            if (currentLog.SumCmsn != 0 && tradesTotal != 0)
            {
               calendarLogs[i].AvgCmsn = currentLog.SumCmsn / tradesTotal;
            }
         }
         return calendarLogs;
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
