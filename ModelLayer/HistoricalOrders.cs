﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   [BsonIgnoreExtraElements]
   public class HistoricalOrders
   {
      public long LinkNumber { get; set; }

      public HistoricalOrders(IEnumerable<HistoricalOrderDto> orders)
      {
         Orders = orders;
      }

      public IEnumerable<HistoricalOrderDto> Orders { get; private set; }

      public IDictionary<string, string> Parameters { get; set; }
   }
}
