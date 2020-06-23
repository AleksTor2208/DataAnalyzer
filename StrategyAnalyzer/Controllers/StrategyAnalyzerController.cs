using ModelLayer;
using StrategyAnalyzer.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace StrategyAnalyzer.Controllers
{
   [RoutePrefix("StrategyAnalysis")]
   public class StrategyAnalyzerController : ApiController
   {
      private readonly IDataProvider _dbProvider;

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
         var result = _dbProvider.GetStrategyInfo(strategyName, currency, timeframe);
         return Ok(new { result });
      }

      [HttpGet]
      [Route("{strategyName}/orders/{currency}/{timeframe}")]
      public IHttpActionResult /*IEnumerable<StrategyResultsDto>*/ GetOrdersInfo(string strategyName, string currency, string timeframe, long id)
      {
         var result = _dbProvider.GetOrdersInfo(strategyName, currency, timeframe, id);
         return Ok(new { result });
      }

      [HttpGet]
      [Route("validation/{strategyName}/{currency}/{timeframe}/{trainPeriod}/{validationPeriod}")]
      public IHttpActionResult GetValidationInfo(string strategyName, string currency, 
                                                 string timeframe, string trainPeriod, string validationPeriod)
      {
         var allHistoryOrders = _dbProvider.GetOrdersInfo(strategyName, currency, timeframe);

         var globalStart = new DateTime(2015, 01, 5);
         var globalEnd = new DateTime(2019, 12, 31);

         var numericTrainPeriod = int.Parse(trainPeriod);
         var numericValidationPeriod = int.Parse(validationPeriod);

         List<SlicePeriod> periodSlices = GetValidationSlices(globalStart, globalEnd, numericTrainPeriod, numericValidationPeriod);

         foreach (var oneSetup in allHistoryOrders)
         {
            // kollekcyja podelennyh na otrezki orderov s ukazannymi periodami
            //ordersSetup - imejetsia v vidu odna nastrojka
            var splittedOrdersSetup = GetSplittedOrdersSetup(oneSetup, periodSlices);
            //splittedOrdersSetup - eto odna nastrojka podelennaja po slajsam.
            //todo:
            //- podschityvat' statistiki dlia kashdogo slajsa, 
            //- znachit k etomu momentu neobhodimyje dannyje dolzhny byt' v baze
         }

         return Ok(new { periodSlices });
      }

      private IEnumerable<SetupSlice> GetSplittedOrdersSetup(HistoricalOrders ordersSetup, List<SlicePeriod> slices)
      {
         //kazhduju nastrojku deliu na otrezki
         var sliceStorage = new List<SetupSlice>();
         foreach (var slice in slices)
         {
            var sliceStorageItem = new SetupSlice();
            sliceStorageItem.PeriodSlice = slice;
            sliceStorageItem.Orders = ordersSetup.Orders.Where(order => DateTime.Compare(order.OpenDate, slice.TrainPeriod.LocalStart) >= 0 &&
                                                                        DateTime.Compare(order.CloseDate, slice.TrainPeriod.LocalEnd) <= 0);

            sliceStorage.Add(sliceStorageItem);
         }
         return sliceStorage;
      }

      private List<SlicePeriod> GetValidationSlices(DateTime globalStart, DateTime globalEnd, int trainPeriod, int validationPeriod)
      {
         bool isFirstSlice = true;
         var slices = new List<SlicePeriod>();
         while (!slices.Any(slice => DateTime.Compare(slice.ValidationPeriod.LocalEnd, globalEnd) > 0))
         {
            if (isFirstSlice)
            {
               var firstSlice = new SlicePeriod();
               firstSlice.TrainPeriod.LocalStart = globalStart;             
               firstSlice.TrainPeriod.LocalEnd = globalStart.AddDays(-1 + trainPeriod * 7);

               firstSlice.ValidationPeriod.LocalStart = firstSlice.TrainPeriod.LocalEnd.AddDays(1);
               firstSlice.ValidationPeriod.LocalEnd = firstSlice.ValidationPeriod.LocalStart.AddDays(-1 + validationPeriod * 7);
               slices.Add(firstSlice);

               isFirstSlice = false;
            }
            else
            {
               var slice = new SlicePeriod();
               slice.TrainPeriod.LocalEnd = slices[slices.Count() - 1].ValidationPeriod.LocalEnd;
               slice.TrainPeriod.LocalStart = slice.TrainPeriod.LocalEnd.AddDays(-7 * trainPeriod + 1);

               slice.ValidationPeriod.LocalStart = slice.TrainPeriod.LocalEnd.AddDays(1);
               slice.ValidationPeriod.LocalEnd = slice.ValidationPeriod.LocalStart.AddDays(-1 + validationPeriod * 7);
               slices.Add(slice);
            }
         }
         return slices;
      }
   }
}
