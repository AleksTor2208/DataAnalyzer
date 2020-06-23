using ModelLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.DAL
{
   public class OrdersDataProvider : IDataProvider
   {
      private readonly IDbConnection _connection;

      public OrdersDataProvider(IDbConnection connection)
      {
         _connection = connection;
      }

      public IEnumerable<HistoricalOrders> GetOrdersInfo(string strategyName, string currency, string hour, long id = long.MinValue)
      {
         return _connection.GetOrdersInfo(strategyName, currency, hour, id);
      }

      public IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         return _connection.GetStrategyInfo(strategyName, currency, hour);
      }
   }
}
