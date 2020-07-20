﻿using System;
using System.Collections.Generic;

namespace MarketDataLoader.Model
{
   public class OrderLog
   {
      public OrderLog()
      {
         Comisions = new List<double>();
      }
      public string Label { get; set; }

      public DateTime OperationDay { get; set; }

      public List<double> Comisions { get; set; }
   }
}
