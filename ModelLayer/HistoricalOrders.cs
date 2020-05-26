using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class HistoricalOrders
   {
      public HistoricalOrders(IEnumerable<HistoricalOrdersDto> orders)
      {
         Orders = orders;
      }

      public IEnumerable<HistoricalOrdersDto> Orders { get; private set; }

      public IDictionary<string, string> Parameters { get; set; }
   }
}
