using ModelLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
   public class OrdersDataProvider : IDataProvider
   {
      private readonly IDbConnection _connection;

      public OrdersDataProvider(IDbConnection connection)
      {
         _connection = connection;
      }

      public IEnumerable<HistoricalOrders> GetOrders(string strategyName, string currency, string hour)
      {
         return _connection.GetOrders(strategyName, currency, hour);
      }

        public async Task<StartEndDateTemplate> GetStartEndDates(string strategyName)
        {
            return await _connection.GetStartEndDates(strategyName);
        }

        public IEnumerable<StrategyTemplate> GetStrategiesDistinct()
        {
            return _connection.GetOrdersDistinct();
        }

        public IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour)
        {
            return _connection.GetStrategyInfo(strategyName, currency, hour);
        }

        public IEnumerable<HistoricalOrders> GetOrdersByStrategyName(string strategyName)
        {
            return _connection.GetOrdersByStrategyName(strategyName);
        }
    }
}
