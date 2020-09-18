using System;
using System.Collections.Generic;

namespace ModelLayer
{
   public class HistoricalOrderDto
   {
      public HistoricalOrderDto()
      {
         Commissions = new List<double>();
      }

      public string Label { get; set; }

      public string Amount { get; set; }

      public string Direction { get; set; }

      public double OpenPrice { get; set; }

      public double ClosePrice { get; set; }

      public double ProfitLoss { get; set; }

      public double ProfitLossInPips { get; set; }

      public DateTime OpenDate { get; set; }

      public DateTime CloseDate { get; set; }

      public DateTime AdjustedOpenDate { get; set; }

      public DateTime AdjustedCloseDate { get; set; }

      public string Comment { get; set; }

      public double DepositCurve { get; set; }

      public double PercentChange { get; set; }

      public List<double> Commissions { get; set; }
   }
}
