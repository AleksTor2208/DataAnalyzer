using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class StrategyResultsDto
   {
      public IEnumerable<StrategyResultDetails> ResultDetails { get; set; }
      public string StrategyName { get; set; }
      public string Currency { get; set; }
      public Timeframe Period { get; set; }
   }
}
