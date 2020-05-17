using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace MarketDataLoader.Converters
{
   class BSonConverter
   {
      internal static BsonDocument GenerateHistoricalOrdersAsBSon(List<HistoricalOrdersDto> historicalOrders, Dictionary<string, string> parameters)
      {
         var bsonDocument = new BsonDocument();
         foreach (var order in historicalOrders)
         {
            var bsonRow = new BsonDocument
            {
               {"Label", order.Label},
               {"Amount", order.Amount},
               {"Direction", order.Direction},
               {"OpenPrice", order.OpenPrice},
               {"ClosePrice", order.ClosePrice},
               {"ProfitLoss", order.ProfitLoss},
               {"ProfitLossInPips", order.ProfitLossInPips},
               {"OpenDate", order.OpenDate},
               {"CloseDate", order.CloseDate},
               {"Comment", order.Comment},
            };
            bsonDocument.AddRange(bsonRow);
         }
         var parametersBSon = new BsonDocument();
         //enrich with parameters
         foreach (var row in parameters)
         {
            parametersBSon.Add(row.Key, row.Value);
         }
         bsonDocument.Add("Parameters", parametersBSon);
         return bsonDocument;
      }

      internal static BsonDocument GenerateOrdersInfoDocument(Dictionary<string, string> account, 
                                                              Dictionary<string, string> parameters, 
                                                              Dictionary<string, string> details)
      {
         var bSonDocument = new BsonDocument();
         foreach (var row in account)
         {
            if (!bSonDocument.Contains(row.Key))
            {
               bSonDocument.Add(row.Key, row.Value);
            }               
         }
         foreach (var row in details)
         {
            if (!bSonDocument.Contains(row.Key))
            {
               bSonDocument.Add(row.Key, row.Value);
            }            
         }
         var parametersBSon = new BsonDocument();
         foreach (var row in parameters)
         {
            parametersBSon.Add(row.Key, row.Value);
         }
         bSonDocument.Add("Parameters", parametersBSon);

         return bSonDocument;
      }
   }
}
