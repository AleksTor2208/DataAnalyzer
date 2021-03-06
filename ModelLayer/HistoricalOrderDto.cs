﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class HistoricalOrderDto
   {
      public string Label { get; set; }

      public string Amount { get; set; }

      public string Direction { get; set; }

      public double OpenPrice { get; set; }

      public double ClosePrice { get; set; }

      public double ProfitLoss { get; set; }

      public double ProfitLossInPips { get; set; }

      public DateTime OpenDate { get; set; }

      public DateTime CloseDate { get; set; }

      public string Comment { get; set; }
   }
}
