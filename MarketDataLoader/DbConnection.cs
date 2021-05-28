using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
//using MongoDB.Driver.Builders;
//using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace MarketDataLoader
{
   class DbConnection
   {
      private readonly IMongoDatabase _db;
      private const string OrdersTableName = "TradeLogs";
      private const string OrdersInfoTableName = "OrdersInfo";
      private const string ResultsInfoTableName = "ResultsInfo";

      public DbConnection()
      {
         var connString = ConfigurationManager.AppSettings["ConnectionString"];
         var dbClient = new MongoClient(connString);
         _db = dbClient.GetDatabase("data");
      }

      public void LoadOrders(BsonDocument bsonDocument)
      {
         var OrdersDb = _db.GetCollection<BsonDocument>(OrdersTableName);
         OrdersDb.InsertOne(bsonDocument);
      }

      public void LoadOrdersInfo(BsonDocument ordersInfo)
      {
         var OrdersInfoDb = _db.GetCollection<BsonDocument>(OrdersInfoTableName);
         OrdersInfoDb.InsertOne(ordersInfo);
      }

      public void CleanOrdersTable()
      {

         //var OrdersDb = _db.GetCollection<BsonDocument>(OrdersTableName);
         //FilterDefinition<TDocument> filter = 
         //OrdersDb.DeleteMany();
      }

      // GetLinkNumber() skoreje vsego nado budet vypelit'

      //internal async Task<long> GetLinkNumber(string strategyName)
      //{
      //   var resultInfoTable = _db.GetCollection<BsonDocument>(ResultsInfoTableName);

      //   var filterForResults = Builders<BsonDocument>.Filter.Eq("StrategyName", strategyName);
      //   var resultInfoData = await resultInfoTable.Find(filterForResults).ToListAsync();
      //   var resultAsList = resultInfoData.Select(r => BsonSerializer.Deserialize<StrategyResultsDto>(r));

      //   var OrdersDb = _db.GetCollection<BsonDocument>(OrdersTableName);
      //   var filterForOrders = Builders<BsonDocument>.Filter.Eq("Comment", strategyName);
      //   var ordersData = await OrdersDb.Find(filterForOrders).ToListAsync();
      //   var ordersAsList = ordersData.Select(r => BsonSerializer.Deserialize<HistoricalOrders>(r));

      //   long currentLinkNumber;
      //   List<long> resultsLinkNumbers;
      //   List<long> ordersLinkNumbers;
      //   if (resultAsList.Any())
      //   {
      //      resultsLinkNumbers = resultAsList.Select(r => r.Id).OrderByDescending(r => r).ToList();
      //      currentLinkNumber = resultsLinkNumbers.First() + 1;
      //   }
      //   else
      //   {
      //      resultsLinkNumbers = new List<long>();
      //      currentLinkNumber = 1;
      //   }
      //   if (ordersAsList.Any())
      //   {
      //      ordersLinkNumbers = ordersAsList.Select(r => r.LinkNumber).OrderByDescending(r => r).ToList();
      //   }
      //   else
      //   {
      //      ordersLinkNumbers = new List<long>();
      //   }

      //   while (resultsLinkNumbers.Contains(currentLinkNumber) || ordersLinkNumbers.Contains(currentLinkNumber))
      //   {
      //      currentLinkNumber++;
      //   }
      //   return currentLinkNumber;
      //}

      internal void LoadDataToResultsTable(BsonDocument resultsAsBSon)
      {
         var OrdersInfoDb = _db.GetCollection<BsonDocument>(ResultsInfoTableName);   
         OrdersInfoDb.InsertOne(resultsAsBSon);
      }
   }
}
