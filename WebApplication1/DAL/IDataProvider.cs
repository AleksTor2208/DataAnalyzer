using ModelLayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
   public interface IDataProvider
   {
      IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour);

      IEnumerable<HistoricalOrders> GetOrders(string strategyName, string currency, string hour);

      IEnumerable<StrategyTemplate> GetStrategiesDistinct();

      Task<StartEndDateTemplate> GetStartEndDates(string strategyName);

      IEnumerable<HistoricalOrders> GetOrdersByStrategyName(string strategyName);
    }
}