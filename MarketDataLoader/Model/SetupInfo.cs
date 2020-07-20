using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataLoader.Model
{
   class SetupInfo
   {
      public int ClosedPositions { get; set; }
      public string StrategyName { get; set; }
      public string AccountCurrency { get; set; }
      public double InitialDeposit { get; set; }
      public double Commission { get; set; }
      public DateTime StartDate { get; set; }
      public DateTime EndDate { get; set; }  
   }
}
