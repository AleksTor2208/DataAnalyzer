using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
//using MongoDB.Driver.Builders;
//using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace MarketDataLoader
{
   class DbConnection
   {
      private readonly IMongoDatabase _db;
      private const string OrdersTableName = "Orders";
      private const string OrdersInfoTableName = "OrdersInfo";

      public DbConnection()
      {
         var connString = ConfigurationManager.AppSettings["ConnectionString"];
         var dbClient = new MongoClient(connString);
         _db = dbClient.GetDatabase("data");
      }

      public void LoadOrders(IEnumerable<BsonDocument> bsonDocuments)
      {
         var OrdersDb = _db.GetCollection<BsonDocument>(OrdersTableName);
         OrdersDb.InsertMany(bsonDocuments);
      }

      public void LoadOrdersInfo(BsonDocument ordersInfo)
      {
         var OrdersInfoDb = _db.GetCollection<BsonDocument>(OrdersInfoTableName);
         OrdersInfoDb.InsertOne(ordersInfo);
      }
   }
}
