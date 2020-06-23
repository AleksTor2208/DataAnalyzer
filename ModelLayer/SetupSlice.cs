using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class SetupSlice
   {
      public SetupSlice()
      {
         Orders = new List<HistoricalOrderDto>();
      }
      public SlicePeriod PeriodSlice { get; set; }
      public IEnumerable<HistoricalOrderDto> Orders { get; set; }
   }
}
