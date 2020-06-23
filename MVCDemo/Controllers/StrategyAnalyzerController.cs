using ModelLayer;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
//using System.Net.Http.Formatting;

namespace MVCDemo.Controllers
{
   //[RoutePrefix("StrategyAnalysis")]
   public class StrategyAnalyzerController : Controller
   {

      public StrategyAnalyzerController()
      {
      }

      //[System.Web.Http.HttpGet]
      //[System.Web.Http.Route("{strategyName}/results/{currency}/{hour}")]
      //public ActionResult GetStrategyInfo(string strategyName, string currency, string hour)
      //{
      //   var result = _dbProvider.GetStrategyInfo(strategyName, currency, hour);
      //   return View("~/Views/Home/Index.cshtml", result);
      //}

      [HttpGet]
      //[Route("{strategyName}/results/{currency}/{hour}")]
      public IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         var list = new List<StrategyResultsDto>();
         var httpClient = new HttpClient();
         httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
         HttpResponseMessage response;
         response = httpClient.GetAsync("http://localhost:50061/StrategyAnalysis/strategyName/results/curreency/timeframe").Result;
         response.EnsureSuccessStatusCode();
         //List<StrategyResultDto> cd = response.Content.ReadAsAsync<List<StrategyResultDto>>().Result;
         //return _dbProvider.GetStrategyInfo(strategyName, currency, hour);
         return new List<StrategyResultsDto>();
      }
   }
}
