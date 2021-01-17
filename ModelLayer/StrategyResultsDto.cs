using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   [BsonIgnoreExtraElements]
   public class StrategyResultsDto
   {
      public double OrdersPerMonth { get; set; }
      public double AnualGrowth { get; set; }
      public double MaxDrawDown { get; set; }
      public double Recovery { get; set; }
      public double RSquared { get; set; }
      public double AvarageOrderInPips { get; set; }      
      public string StrategyName { get; set; }
      public string Currency { get; set; }
      public string Timeframe { get; set; }
      public IDictionary<string, string> Parameters { get; set; }
   }
}
