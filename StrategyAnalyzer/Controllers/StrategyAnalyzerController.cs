using StrategyAnalyzer.DAL;
using StrategyAnalyzer.Model;
using System.Collections.Generic;
using System.Web.Http;

namespace StrategyAnalyzer.Controllers
{
   [RoutePrefix("strategyanalysis")]
   public class StrategyAnalyzerController : ApiController
   {
      private readonly IDataProvider _dbProvider;

      public StrategyAnalyzerController(IDataProvider dbProvider)
      {
         _dbProvider = dbProvider;
      }

      [HttpGet]
      [Route("{strategyName}/results/{currency}/{hour}")]
      public IEnumerable<StrategyResultDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         return _dbProvider.GetStrategyInfo(strategyName, currency, hour);
      }

      //[HttpGet]
      //[Route("results/{strategyName}/{currency}/{hour}")]
   }
}
