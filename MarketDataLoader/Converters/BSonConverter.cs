using ModelLayer;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace MarketDataLoader.Converters
{
   class BSonConverter
   {
      internal static BsonDocument GenerateHistoricalOrdersAsBSon(List<HistoricalOrdersDto> historicalOrders, Dictionary<string, string> parameters)
      {
         var historicalOrdersList = new HistoricalOrders(historicalOrders);
         historicalOrdersList.Parameters = parameters;
         return historicalOrdersList.ToBsonDocument();
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
