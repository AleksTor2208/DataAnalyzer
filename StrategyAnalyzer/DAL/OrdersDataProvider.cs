using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrategyAnalyzer.Model;

namespace StrategyAnalyzer.DAL
{
   public class OrdersDataProvider : IDataProvider
   {
      private readonly IDbConnection _connection;

      public OrdersDataProvider(IDbConnection connection)
      {
         _connection = connection;
      }

      public IEnumerable<StrategyResultDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         return _connection.GetStrategyInfo(strategyName, currency, hour);
      }
   }
}
