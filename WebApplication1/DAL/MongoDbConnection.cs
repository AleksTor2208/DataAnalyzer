using ModelLayer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer;
using WebApplication1.Models;
using System.Linq.Expressions;

namespace WebApplication1.DAL
{
   public class MongoDbConnection : IDbConnection
   {
      private readonly IMongoDatabase _db;
      private const string OrdersTableName = "Orders";
      private const string OrdersInfoTableName = "OrdersInfo";
      private const string ResultsInfoTableName = "ResultsInfo";

      public MongoDbConnection()
      {
         var connString = ConfigurationManager.AppSettings["ConnectionString"];
         var dbClient = new MongoClient(connString);
         _db = dbClient.GetDatabase("data");
      }

      public IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour)
      {
         var ResultsInfo = _db.GetCollection<BsonDocument>(ResultsInfoTableName);
         //var filter = Builders<StrategyResultsDto>.Filter.Where(e => e.StrategyName == strategyName).ToBsonDocument();
         //&& e.Currency.Replace("/", "") == currency && e.Period.ToString().Replace(" ", "") == hour).ToBsonDocument();

         var filter = Builders<BsonDocument>.Filter.Eq("StrategyName", strategyName);
         var results = ResultsInfo.Find(filter).ToList();
         var resultAsList = results.Select(r => BsonSerializer.Deserialize<StrategyResultsDto>(r));
         return resultAsList.Where(r => r.Currency.Replace("/", "").Equals(currency, StringComparison.InvariantCultureIgnoreCase)
                              && r.Timeframe.Replace(" ", "").Equals(hour, StringComparison.InvariantCultureIgnoreCase));
      }

      public IEnumerable<HistoricalOrders> GetOrders(string strategyName, string currency, string hour)
      {
         var ordersTable = _db.GetCollection<BsonDocument>(OrdersTableName);
         List<BsonDocument> results;

         results = ordersTable.Find(_ => true).ToList();
         //var filter = Builders<BsonDocument>.Filter.Eq("LinkNumber", id);
         //results = ordersTable.Find(filter).ToList();
         var ordersAsList = results.Select(r => BsonSerializer.Deserialize<HistoricalOrders>(r));
         return ordersAsList;
      }

        public IEnumerable<StrategyTemplate> GetOrdersDistinct()
        {
            //var ordersTable = _db.GetCollection<BsonDocument>(OrdersTableName);
            //var results = ordersTable.Find(_ => true).ToList();
            //var ordersAsList = results.Select(r => BsonSerializer.Deserialize<HistoricalOrders>(r));

            //return ordersAsList.Select(e => e.StrategyName).Distinct().Select(e => new StrategyTemplate(e));
            return GetFieldValue<HistoricalOrders, string>(x => x.StrategyName).Distinct().Select(x => new StrategyTemplate(x));
        }

        public async Task<StartEndDateTemplate> GetStartEndDates(string strategyName)
        {
            var startDate = await GetFieldValue<HistoricalOrders, DateTime>(strategyName, x => x.Orders.Select(o => o.OpenDate).Min());
            var lastDate = await GetFieldValue<HistoricalOrders, DateTime>(strategyName, x => x.Orders.Select(o => o.CloseDate).Max());
            return new StartEndDateTemplate(startDate, lastDate);
        }

        public IEnumerable<HistoricalOrders> GetOrdersByStrategyName(string strategyName)
        {
            return _db.GetCollection<HistoricalOrders>(OrdersTableName)
                .Find(d => d.StrategyName == strategyName)
                .Project(new ProjectionDefinitionBuilder<HistoricalOrders>().Expression(x => x)).ToList();
        }

        private async Task<TValue> GetFieldValue<TEntity, TValue>(string strategyName, Expression<Func<TEntity, TValue>> fieldExpression) where TEntity : HistoricalOrders
        {
            var propertyValue = await _db.GetCollection<TEntity>(OrdersTableName)
                .Find(d => d.StrategyName == strategyName)
                .Project(new ProjectionDefinitionBuilder<TEntity>().Expression(fieldExpression))
                .FirstOrDefaultAsync();

            return propertyValue;
        }

        private IEnumerable<TValue> GetFieldValue<TEntity, TValue>(Expression<Func<TEntity, TValue>> fieldExpression) where TEntity : HistoricalOrders
        {
            var propertyValue = _db.GetCollection<TEntity>(OrdersTableName)
                .Find(d => true)
                .Project(new ProjectionDefinitionBuilder<TEntity>().Expression(fieldExpression)).ToList();

            return propertyValue;
        }
    }
}
