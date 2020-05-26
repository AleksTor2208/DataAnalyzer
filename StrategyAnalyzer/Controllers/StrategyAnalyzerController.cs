using StrategyAnalyzer.DAL;
using StrategyAnalyzer.Model;
using System.Collections.Generic;
using System.Web.Http;
using MVCDemo.Controllers;

namespace StrategyAnalyzer.Controllers
{
   [System.Web.Http.RoutePrefix("StrategyAnalysis")]
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
      public IEnumerable<StrategyResultDto> GetStrategyInfo(string strategyName, string currency, string timeframe)
      {
         return _dbProvider.GetStrategyInfo(strategyName, currency, timeframe);
      }
   }
}
