using log4net;
using log4net.Appender;
using log4net.Repository;
using MarketDataLoader.Converters;
using MarketData.Common.ExtensionMethods;
using MarketData.Common;
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

            var historicalOrders = _htmlReader.ReadHistoricalOrders(TableType.ClosedOrders, 
                                                   OrdersTableHeaders.ClosedOrdersHeaders, new ClosedOrdersConverter());

            var openedOrders = _htmlReader.ReadHistoricalOrders(TableType.OpenedOrders,
                                                   OrdersTableHeaders.OpenedOrdersHeaders, new OpenedOrdersConverter());
            if (openedOrders.Any())
            {
               historicalOrders.AddRange(openedOrders);
            }

            var currency = _htmlReader.GetCurrency();
            var strategyName = _htmlReader.GetStrategyName();//historicalOrders.First().Comment;

            long linkNumber = _dbConnection.GetLinkNumber(strategyName).Result;

            var basicInfo = _htmlReader.ReadDetailsTables(TableType.BasicInfo);
            var paramsInfo = _htmlReader.ReadDetailsTables(TableType.ParamsInfo);
            var detailsInfo = _htmlReader.ReadDetailsTables(TableType.DetailsInfo);

            // setupInfo is agreggated data for preparing calendarLogs
            SetupInfo setupInfo = PrepareSetupInfo(htmlFile, basicInfo, paramsInfo, detailsInfo, strategyName, currency);

            SetupAdjustedOrderDates(historicalOrders);
            var calendarLogs = SetupCalendarLogs(historicalOrders, orderLogs, setupInfo);

            EnrichHistoricalOrdersWithCommissions(calendarLogs, historicalOrders);

            //Calculate Deposit curve separately because it will be needed to calculate drowdowns!
            CalculateDepositCurve(historicalOrders, setupInfo.InitialDeposit);

            CalculatePercentChange(historicalOrders, setupInfo.InitialDeposit);

            // should be modified, commissions should be already calculated in Enrichment step
            var strategyResultsConverter = new StrategyResultsCalculator(basicInfo, paramsInfo, detailsInfo, setupInfo, calendarLogs);
            var strategyResultsAsBSon = strategyResultsConverter.Calculate(historicalOrders, orderLogs, paramsInfo, linkNumber).ToBsonDocument();

            //GenerateHistoricalOrdersAsTable(historicalOrders);
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

      private void GenerateHistoricalOrdersAsTable(List<HistoricalOrderDto> historicalOrders)
      {
         using (StreamWriter file = new StreamWriter(@"C:\Users\ASUS\Documents\FX\sample_reports\test\will_sar_xover_v2_TradeLogs.csv"))
         {
            file.WriteLine("Direction, OpenPrice, ClosePrice, Profit/Loss, Profit/Loss in pips, Open Date, Close Date, " +
               "First Commission, Second Commission, Deposit Curve, Percent Change, Adjusted Open Date, Adjusted Close Date");
            foreach (var order in historicalOrders.ToArray())
            {
               if (Array.IndexOf(historicalOrders.ToArray(), order) == historicalOrders.Count() - 1)
               {
                  continue;
               }
               file.WriteLine($"{order.Direction}, {order.OpenPrice}, {order.ClosePrice}, " +
                  $"{order.ProfitLoss}, {order.ProfitLossInPips}, {order.OpenDate}, {order.CloseDate}, " +
                  $"{order.Commissions[0]}, {order.Commissions[1]}, {order.DepositCurve}, " +
                  $"{order.PercentChange}, {order.AdjustedOpenDate}, {order.AdjustedCloseDate}");
            }
         }
      }

      private void CalculateDepositCurve(List<HistoricalOrderDto> historicalOrders, double initialDeposit)
      {
         bool isFirstOrder = true;
         double previousDepositCurve = initialDeposit;
         foreach (var trade in historicalOrders)
         {
            trade.DepositCurve = previousDepositCurve + trade.ProfitLoss - trade.Commissions.Sum();
            previousDepositCurve = trade.DepositCurve;
         }
      }

      private void CalculatePercentChange(List<HistoricalOrderDto> historicalOrders, double initialDeposit)
      {
         double previousDepositCurve = initialDeposit;
         foreach (var trade in historicalOrders)
         {
            trade.PercentChange = (trade.DepositCurve - previousDepositCurve) / previousDepositCurve;
            previousDepositCurve = trade.DepositCurve;
         }
      }

      private void EnrichHistoricalOrdersWithCommissions(List<CalendarLog> calendarLogs, List<HistoricalOrderDto> historicalOrders)
      {
         foreach (var calendarLog in calendarLogs)
         {
            foreach (var trade in historicalOrders)
            {
               if (calendarLog.TradesClosed.Contains(trade.Label))
               {
                  trade.Commissions.Add(calendarLog.AvgCmsn);
               }
               if (calendarLog.TradesOpened.Contains(trade.Label))
               {
                  trade.Commissions.Add(calendarLog.AvgCmsn);
               }
            }
         }
      }

      private void SetupAdjustedOrderDates(List<HistoricalOrderDto> historicalOrders)
      {
         //if order.OpenDate.DayOfWeek
         // OpenDate/ClosedDate nado ostavit'!!!
         
         const int LastHour = 21;
         foreach (var order in historicalOrders)
         {
            var oDate = order.OpenDate;
            if (DateTime.Compare(order.OpenDate, new DateTime(oDate.Year, oDate.Month, oDate.Day, LastHour, 0, 0)) > 0)
            {
               var tempDate = order.OpenDate.AddDays(1);
               order.AdjustedOpenDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day);
            }
            else
            {
               order.AdjustedOpenDate = order.OpenDate;
            }
            var cDate = order.CloseDate;
            if (DateTime.Compare(order.CloseDate, new DateTime(cDate.Year, cDate.Month, cDate.Day, LastHour, 0, 0)) > 0)
            {
               var tempDate = order.CloseDate.AddDays(1);
               order.AdjustedCloseDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day);
            }
            else
            {
               order.AdjustedCloseDate = order.CloseDate;
            }
         }
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

      private List<CalendarLog> SetupCalendarLogs(IEnumerable<HistoricalOrderDto> historicalOrders, 
                                                             IEnumerable<OrderLog> orderLogs, SetupInfo setupInfo)
      {
         var calendarLogs = new List<CalendarLog>();
         DateTime currentdate = setupInfo.StartDate.AddDays(-1);

         //preparing calendarLogs skeleton 
         while (DateTime.Compare(currentdate, setupInfo.EndDate) <= 0)
         {
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

         //TradesOpened\TradesClosed set
         foreach (var calendarLog in calendarLogs)
         {
            foreach (var currentOpenedOrder in historicalOrders.Where(order => DateTime.Compare(
                                                               order.AdjustedOpenDate.Date, calendarLog.OperationDay.Date) == 0))
            {
               calendarLog.TradesOpened.Add(currentOpenedOrder.Label);
            }

            foreach (var currentClosedOrder in historicalOrders.Where(order => DateTime.Compare(
                                                                        order.AdjustedCloseDate.Date, calendarLog.OperationDay.Date) == 0))
            {
               calendarLog.TradesClosed.Add(currentClosedOrder.Label);
            }
         }

         //AvgCmsn = сумма_комиссий / (сделок_открыто + сделок_закрыто)
         foreach (var calendarLog in calendarLogs)
         {
            int tradesTotal = calendarLog.TradesOpened.Count() + calendarLog.TradesClosed.Count();
            if (calendarLog.SumCmsn != 0 && tradesTotal != 0)
            {
               calendarLog.AvgCmsn = calendarLog.SumCmsn / tradesTotal;
            }
         }
         SetupEodResult(calendarLogs, setupInfo.StartDate, setupInfo.InitialDeposit, historicalOrders);
         return calendarLogs;
      }

      private void SetupEodResult(List<CalendarLog> calendarLogs, DateTime startDate, double initialDeposit, IEnumerable<HistoricalOrderDto> historicalOrders)
      {
         double currentEodResult = initialDeposit;
         foreach (var calendarLog in calendarLogs)
         {
            if (DateTime.Compare(calendarLog.OperationDay, startDate) < 0)
            {
               calendarLog.EodFinRes = currentEodResult;
               continue;
            }
            //checking 
            var pickedOrders = historicalOrders.Where(currentOrder => DateTime.Compare(currentOrder.CloseDate.Date, calendarLog.OperationDay.Date) == 0);
            if (!pickedOrders.Any())
            {
               calendarLog.EodFinRes = currentEodResult;
            }
            double totalProfitLoss = 0;
            foreach (var order in pickedOrders)
            {
               totalProfitLoss += order.ProfitLoss;
            }
            currentEodResult = currentEodResult + totalProfitLoss - calendarLog.SumCmsn;
            calendarLog.EodFinRes = currentEodResult;
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
