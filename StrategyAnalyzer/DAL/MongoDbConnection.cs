﻿using ModelLayer;
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

namespace StrategyAnalyzer.DAL
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
         var OrdersInfoDb = _db.GetCollection<BsonDocument>(ResultsInfoTableName);
         //var filter = Builders<StrategyResultsDto>.Filter.Where(e => e.StrategyName == strategyName).ToBsonDocument();
         //&& e.Currency.Replace("/", "") == currency && e.Period.ToString().Replace(" ", "") == hour).ToBsonDocument();

         var filter = Builders<BsonDocument>.Filter.Eq("StrategyName", strategyName);

         var results = OrdersInfoDb.Find(filter).ToList();
         var resultAsList = results.Select(r => BsonSerializer.Deserialize<StrategyResultsDto>(r));

         return resultAsList;
      }
   }
}
