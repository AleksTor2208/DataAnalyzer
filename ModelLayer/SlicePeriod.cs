using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
   public class SlicePeriod
   {
      public SlicePeriod()
      {
         TrainPeriod = new Period();
         ValidationPeriod = new Period();
      }

      public Period TrainPeriod { get; set; }
      public Period ValidationPeriod { get; set; }
   }

   public class Period
   {
      public DateTime LocalStart { get; set; }
      public DateTime LocalEnd { get; set; }
   }
}
