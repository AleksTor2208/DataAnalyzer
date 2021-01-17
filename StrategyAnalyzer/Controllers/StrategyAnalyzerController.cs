using log4net;
using ModelLayer;
using ModelLayer.Validation;
using StrategyAnalyzer.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MarketData.Common;

namespace StrategyAnalyzer.Controllers
{
   [RoutePrefix("StrategyAnalysis")]
   public class StrategyAnalyzerController : ApiController
   {
      private readonly IDataProvider _dbProvider;
      private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      public StrategyAnalyzerController(IDataProvider dbProvider)
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
                                                 string timeframe, string trainPeriod, string validationPeriod)
      {
         var allHistoryOrders = _dbProvider.GetOrders(strategyName, currency, timeframe);

         var globalStart = new DateTime(2015, 01, 5);
         var globalEnd = new DateTime(2019, 12, 31);

         //needs validation
         var numericTrainPeriod = int.Parse(trainPeriod);
         var numericValidationPeriod = int.Parse(validationPeriod);

         List<SlicePeriod> periodSlices = Validation.GetValidationSlices(globalStart, globalEnd, numericTrainPeriod, numericValidationPeriod);

         //this is what should be done
         foreach (var slice in periodSlices)
         {

         }


         foreach (var oneSetup in allHistoryOrders)
         {
            // kollekcyja podelennyh na otrezki orderov s ukazannymi periodami
            //ordersSetup - imejetsia v vidu odna nastrojka
            var splittedOrdersSetup = GetSplittedOrdersSetup(oneSetup, periodSlices);
            //splittedOrdersSetup - eto odna nastrojka podelennaja po slajsam.
            //todo:
            //- podschityvat' statistiki dlia kashdogo slajsa, 
         }
         return Ok(new { periodSlices });
      }

      private IEnumerable<SetupSlice> GetSplittedOrdersSetup(HistoricalOrders oneSetup, List<SlicePeriod> slices)
      {
         //dlia kazhdogo vremennogo otrezka po zadannoj nastrojke nahozhu sootvetstvujushchije ordera 
         var setupSlices = new List<SetupSlice>();
         foreach (var slice in slices)
         {
            var sliceStorageItem = new SetupSlice();
            sliceStorageItem.Slice = slice;

            sliceStorageItem.TrainingOrders = oneSetup.Orders.Where(order => DateTime.Compare(order.AdjustedOpenDate, slice.TrainPeriod.LocalStart) >= 0 &&
                                                                        DateTime.Compare(order.AdjustedCloseDate, slice.TrainPeriod.LocalEnd) <= 0);

            sliceStorageItem.ValidationOrders = oneSetup.Orders.Where(order => DateTime.Compare(order.AdjustedOpenDate, slice.ValidationPeriod.LocalStart) >= 0 &&
                                                                        DateTime.Compare(order.AdjustedCloseDate, slice.ValidationPeriod.LocalEnd) <= 0);



            //var StrategyResultsCalculator = new StrategyResultsCalculator();
            //var annualGrowth = CalculateAnnualGrowth(historicalOrders);
            //var maximumDrowDown = CalculateMaximumDrowdown(historicalOrders);
            //var recovery = CalculateRecovery(annualGrowth, maximumDrowDown);
            //var rSquared = CalculateRSquared(calendarLogs);


            sliceStorageItem.Parameters = oneSetup.Parameters;




            setupSlices.Add(sliceStorageItem);

         }
         return setupSlices;
      }     
   }
}
