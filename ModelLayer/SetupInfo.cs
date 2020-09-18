using System;

namespace ModelLayer
{
   public class SetupInfo
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
