using log4net;
using ModelLayer;
using ModelLayer.Validation;
using StrategyAnalyzer.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MarketData.Common;
using System.IO;
using StrategyAnalyzer.Helpers;
using CsvHelper;
using System.Globalization;

namespace StrategyAnalyzer.Controllers
{
    [RoutePrefix("StrategyExprorer")]
    public class StrategyExprorerController : ApiController
    {
        private readonly IDataProvider _dbProvider;
        private static readonly ILog Log =
                LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StrategyExprorerController(IDataProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("{strategyName}/results/{currency}/{hour}")]
        //public ActionResult GetStrategyInfo(string strategyName, string currency, string hour)
        //{
        //   var result = _dbProvider.GetStrategyInfo(strategyName, currency, hour);
        //   return View("~/Views/Home/Index.cshtml", result);
        //}

        [HttpGet]
        [Route("{strategyName}/results/{currency}/{timeframe}")]
        public IHttpActionResult /*IEnumerable<StrategyResultsDto>*/ GetStrategyResultsInfo(string strategyName, string currency, string timeframe)
        {
            Log.Info("I'm inside GetStrategyResultsInfo()");
            var result = _dbProvider.GetStrategyInfo(strategyName, currency, timeframe);
            return Ok(new { result });
        }

        [HttpGet]
        [Route("{strategyName}/orders/{currency}/{timeframe}")]
        public IHttpActionResult /*IEnumerable<StrategyResultsDto>*/ GetOrdersInfo(string strategyName, string currency, string timeframe)
        {
            Log.Info("I'm inside GetOrdersInfo()");
            var result = _dbProvider.GetOrders(strategyName, currency, timeframe);
            Log.InfoFormat("Orders data had been returns. Amount number: {0}", result.Count());
            return Ok(new { result });
        }

        [HttpGet]
        [Route("{strategyName}/validation/{currency}/{timeframe}/{trainPeriod}/{validationPeriod}")]
        public IHttpActionResult GetValidationInfo(string strategyName, string currency,
                                                   string timeframe, string trainPeriod, string validationPeriod,
                                                    DateTime globalStart, DateTime globalEnd)
        {
            //var allHistoryOrders = _dbProvider.GetOrders(strategyName, currency, timeframe);
            var allHistoryOrders = new List<HistoricalOrders>();
            string path = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData\csv";
            foreach (var file in Directory.EnumerateFiles(path, "*.csv"))
            {
                if (file.Contains("historicalOrders"))
                {
                    var tradeLog = ReadData(file);

                    allHistoryOrders.Add(new TradeLogToHistoryOrders(tradeLog).Convert());
                }
            }
            //var globalStart = new DateTime(2015, 01, 5);
            //var globalEnd = new DateTime(2019, 12, 31);

            //needs validation
            var numericTrainPeriod = int.Parse(trainPeriod);
            var numericValidationPeriod = int.Parse(validationPeriod);

            List<SlicePeriod> periodSlices = Validation.GetValidationSlices(globalStart, globalEnd, numericTrainPeriod, numericValidationPeriod);
            var slicedSetupsBasket = new List<SlicedSetup>();
            foreach (var oneSetup in allHistoryOrders)
            {
                //this is what should be done
                foreach (var slice in periodSlices)
                {
                    var slicedSetup = GetSplittedOrdersSetup(oneSetup, slice);
                    slicedSetupsBasket.Add(slicedSetup);
                }
                // kollekcyja podelennyh na otrezki orderov s ukazannymi periodami
                //ordersSetup - imejetsia v vidu odna nastrojka

                //splittedOrdersSetup - eto odna nastrojka podelennaja po slajsam.
                //todo:
                //- podschityvat' statistiki dlia kashdogo slajsa, 
            }

            var basketAsDictioanry = slicedSetupsBasket.GroupBy(s => s.TimePeriod)
                                                       .ToDictionary(s => s.Key, s => s.ToList());

            foreach (var sliceSetupTimePeriod in basketAsDictioanry.Keys)
            {

            }


            return Ok(new { periodSlices });
        }

        private SlicedSetup GetSplittedOrdersSetup(HistoricalOrders oneSetup, SlicePeriod slice)
        {
            var sliceStorageItem = new SlicedSetup();
            sliceStorageItem.TimePeriod = slice;
            //dlia kazhdogo vremennogo otrezka po zadannoj nastrojke nahozhu sootvetstvujushchije ordera
            sliceStorageItem.TrainingOrders = oneSetup.Orders.Where(order => DateTime.Compare(order.AdjustedOpenDate, slice.TrainPeriod.LocalStart) >= 0 &&
                                                                        DateTime.Compare(order.AdjustedCloseDate, slice.TrainPeriod.LocalEnd) <= 0).ToList();

            sliceStorageItem.ValidationOrders = oneSetup.Orders.Where(order => DateTime.Compare(order.AdjustedOpenDate, slice.ValidationPeriod.LocalStart) >= 0 &&
                                                                        DateTime.Compare(order.AdjustedCloseDate, slice.ValidationPeriod.LocalEnd) <= 0).ToList();

            sliceStorageItem.Parameters = oneSetup.Parameters;

            CalculateStatisticsForSlicedSetup(sliceStorageItem);
            return sliceStorageItem;
        }

        private void CalculateStatisticsForSlicedSetup(SlicedSetup slicedSetup)
        {
            var strategyResultsCalculator = new StrategyResultsCalculator();
            var annualGrowthTO = strategyResultsCalculator.CalculateAnnualGrowth(slicedSetup.TrainingOrders.ToList());
            var maximumDrowDownTO = strategyResultsCalculator.CalculateMaximumDrowdown(slicedSetup.TrainingOrders.ToList());
            var recoveryTO = strategyResultsCalculator.CalculateRecovery(annualGrowthTO, maximumDrowDownTO);
            //var rSquared = CalculateRSquared(calendarLogs);

            slicedSetup.TrainingOrdersStatistics = new Statistics
            {
                AnualGrowth = annualGrowthTO,
                MaxDrawDown = maximumDrowDownTO,
                Recovery = recoveryTO
            };

            var annualGrowthVO = strategyResultsCalculator.CalculateAnnualGrowth(slicedSetup.ValidationOrders.ToList());
            var maximumDrowDownVO = strategyResultsCalculator.CalculateMaximumDrowdown(slicedSetup.ValidationOrders.ToList());
            var recoveryVO = strategyResultsCalculator.CalculateRecovery(annualGrowthVO, maximumDrowDownVO);

            slicedSetup.ValidationOrdersStatistics = new Statistics
            {
                AnualGrowth = annualGrowthVO,
                MaxDrawDown = maximumDrowDownVO,
                Recovery = recoveryVO
            };
        }

        private List<TradeLogDto> ReadData(string path)
        {
            List<TradeLogDto> tradeLogs = new List<TradeLogDto>();
            if (System.IO.File.Exists(path))
            {
                FileStream fileStream = File.OpenRead(path);
                using (var reader = new StreamReader(fileStream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<CSVFileDefinitionMap>();
                    while (csv.Read())
                    {
                        //var record = csv.GetRecord<TradeLogDto>();

                        //csv.Configuration.HasHeaderRecord = true;
                        //csv.Configuration.MissingFieldFound.
                        var record = csv.GetRecord<TradeLogDto>();
                        tradeLogs.Add(record);
                    }
                }
            }
            return tradeLogs;
        }
    }
}
