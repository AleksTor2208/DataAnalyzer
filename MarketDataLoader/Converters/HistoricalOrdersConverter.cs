using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataLoader.Converters
{
   class HistoricalOrdersConverter
   {
      internal HistoricalOrdersDto Convert(string row)
      {
         var rowAsArray = row.Replace("<td>", "").Split(new[] { "</td>" },
                                              StringSplitOptions.RemoveEmptyEntries);
         var order = new HistoricalOrdersDto
         {
            Label = rowAsArray[0],
            Amount = rowAsArray[1],
            Direction = rowAsArray[2],
            OpenPrice = rowAsArray[3],
            ClosePrice = rowAsArray[4],
            ProfitLoss = rowAsArray[5],
            ProfitLossInPips = rowAsArray[6],
            OpenDate = rowAsArray[7],
            CloseDate = rowAsArray[8],
            Comment = rowAsArray[9],
         };
         return order;
      }
   }
}
  

