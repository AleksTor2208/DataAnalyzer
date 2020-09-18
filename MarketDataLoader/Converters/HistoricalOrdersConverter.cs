using MarketDataLoader.ExtensionMethods;
using ModelLayer;

namespace MarketDataLoader.Converters
{
   internal interface IOrdersConverter
   {
      HistoricalOrderDto Convert(string row);
   }

   internal class ClosedOrdersConverter : IOrdersConverter
   {
      public HistoricalOrderDto Convert(string row)
      {
         var rowAsArray = row.Replace("<td>", "").Split("</td>");
         var order = new HistoricalOrderDto
         {
            Label = rowAsArray[0],
            Amount = rowAsArray[1],
            Direction = rowAsArray[2],
            OpenPrice = rowAsArray[3].ToDouble(),
            ClosePrice = rowAsArray[4].ToDouble(),
            ProfitLoss = rowAsArray[5].ToDouble(),
            ProfitLossInPips = rowAsArray[6].ToDouble(),
            OpenDate = rowAsArray[7].ToDateTime(),
            CloseDate = rowAsArray[8].ToDateTime(),
            Comment = rowAsArray[9],
         };
         return order;
      }

   }

   internal class OpenedOrdersConverter : IOrdersConverter
   {
      public HistoricalOrderDto Convert(string row)
      {
         var rowAsArray = row.Replace("<td>", "").Split("</td>");
         var order = new HistoricalOrderDto
         {
            Label = rowAsArray[0],
            Amount = rowAsArray[1],
            Direction = rowAsArray[2],
            OpenPrice = rowAsArray[3].ToDouble(),
            ProfitLoss = rowAsArray[4].ToDouble(),
            ProfitLossInPips = rowAsArray[5].ToDouble(),
            OpenDate = rowAsArray[6].ToDateTime(),
            Comment = rowAsArray[7],
         };
         return order;
      }
   }
}
  

