using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataLoader
{
   class HistoricalOrdersDto
   {
      public string Label { get; set; }

      public string Amount { get; set; }

      public string Direction { get; set; }

      public string OpenPrice { get; set; }

      public string ClosePrice { get; set; }

      public string ProfitLoss { get; set; }

      public string ProfitLossInPips { get; set; }

      public string OpenDate { get; set; }

      public string CloseDate { get; set; }

      public string Comment { get; set; }
   }
}
