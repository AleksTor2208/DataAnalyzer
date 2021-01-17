using ModelLayer.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer
{
   class Validation
   {
      internal static List<SlicePeriod> GetValidationSlices(DateTime globalStart, DateTime globalEnd, int trainPeriod, int validationPeriod)
      {
         bool isFirstSlice = true;
         var slices = new List<SlicePeriod>();
         while (!slices.Any(slice => DateTime.Compare(slice.ValidationPeriod.LocalEnd, globalEnd) > 0))
         {
            if (isFirstSlice)
            {
               var firstSlice = new SlicePeriod();
               firstSlice.TrainPeriod.LocalStart = globalStart;
               firstSlice.TrainPeriod.LocalEnd = globalStart.AddDays(-1 + trainPeriod * 7);

               firstSlice.ValidationPeriod.LocalStart = firstSlice.TrainPeriod.LocalEnd.AddDays(1);
               firstSlice.ValidationPeriod.LocalEnd = firstSlice.ValidationPeriod.LocalStart.AddDays(-1 + validationPeriod * 7);
               slices.Add(firstSlice);

               isFirstSlice = false;
            }
            else
            {
               var slice = new SlicePeriod();
               slice.TrainPeriod.LocalEnd = slices[slices.Count() - 1].ValidationPeriod.LocalEnd;
               slice.TrainPeriod.LocalStart = slice.TrainPeriod.LocalEnd.AddDays(-7 * trainPeriod + 1);

               slice.ValidationPeriod.LocalStart = slice.TrainPeriod.LocalEnd.AddDays(1);
               slice.ValidationPeriod.LocalEnd = slice.ValidationPeriod.LocalStart.AddDays(-1 + validationPeriod * 7);
               slices.Add(slice);
            }
         }
         return slices;
      }
   }
}
