using ModelLayer;
using System.Collections.Generic;

namespace StrategyAnalyzer.DAL
{
   public interface IDataProvider
   {
      IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour);

      IEnumerable<HistoricalOrders> GetOrdersInfo(string strategyName, string currency, string hour, long id = long.MinValue);
   }
}