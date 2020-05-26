using StrategyAnalyzer.Model;
using System.Collections.Generic;

namespace StrategyAnalyzer.DAL
{
   public interface IDbConnection
   {
      IEnumerable<StrategyResultDto> GetStrategyInfo(string strategyName, string currency, string hour);
   }
}