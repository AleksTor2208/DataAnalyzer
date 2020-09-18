using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class CalendarLog
   {
      public CalendarLog()
      {
         TradesOpened = new List<string>();
         TradesClosed = new List<string>();
      }

      public DateTime OperationDay { get; set; }
      public List<string> TradesOpened { get; set; }
      public List<string> TradesClosed { get; set; }
      public double SumCmsn { get; set; }
      public double AvgCmsn { get; set; }
      public double EodFinRes { get; set; }
   }
}
