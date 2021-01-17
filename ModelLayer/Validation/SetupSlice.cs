using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Validation
{
   public class SetupSlice
   {
      public SetupSlice()
      {
         TrainingOrders = new List<HistoricalOrderDto>();
         ValidationOrders = new List<HistoricalOrderDto>();
         Parameters = new Dictionary<string, string>();
      }

      public SlicePeriod Slice { get; set; }
      public IEnumerable<HistoricalOrderDto> TrainingOrders { get; set; }
      public Statistics TrainingOrdersStatistics { get; set; }
      public IEnumerable<HistoricalOrderDto> ValidationOrders { get; set; }
      public Statistics ValidationOrdersStatistics { get; set; }
      public IDictionary<string, string> Parameters { get; set; }
   }
}
