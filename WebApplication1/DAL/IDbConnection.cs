using ModelLayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
   public interface IDbConnection
   {
      IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour);

      IEnumerable<HistoricalOrders> GetOrders(string strategyName, string currency, string hour);
      
      IEnumerable<StrategyTemplate> GetOrdersDistinct();

      Task<StartEndDateTemplate> GetStartEndDates(string strategyName);

      IEnumerable<HistoricalOrders> GetOrdersByStrategyName(string strategyName);
   }
}