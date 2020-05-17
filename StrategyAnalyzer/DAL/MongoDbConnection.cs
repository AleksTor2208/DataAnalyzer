using MongoDB.Driver;
using StrategyAnalyzer.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.DAL
{
   public class MongoDbConnection : IDbConnection
   {
      private readonly IMongoDatabase _db;
      private const string OrdersTableName = "Orders";
      private const string OrdersInfoTableName = "OrdersInfo";

      public MongoDbConnection()
      {
         var connString = ConfigurationManager.AppSettings["ConnectionString"];
         var dbClient = new MongoClient(connString);
         _db = dbClient.GetDatabase("data");
      }

      public IEnumerable<StrategyResultDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         return new List<StrategyResultDto>();
      }
   }
}
