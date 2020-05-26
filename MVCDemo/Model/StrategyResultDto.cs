using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.Model
{
   public class StrategyResultDto
   {
      public string LinkNumber { get; set; }
      public double OrdersQuantity { get; set; }
      public double AnualGrowth { get; set; }
      public double MaxDrawDown { get; set; }
      public double Recovery { get; set; }
      public double RSquared { get; set; }
      public double AvarageOrder { get; set; }
      public string StrategyName { get; set; }
      public string Currency { get; set; }
      public Timeframe Period { get; set; }
      public IEnumerable<string> Parameters { get; set; }
   }
}
