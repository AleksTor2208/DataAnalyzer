namespace MarketDataLoader
{
   internal static class OrdersTableHeaders
   {
      public static string[] ClosedOrdersHeaders
      {
         get
         {
            return new string[]
            {
               "Label",
               "Amount",
               "Direction",
               "Open price",
               "Close price",
               "Profit/Loss",
               "Profit/Loss in pips",
               "Open date",
               "Close date",
               "Comment"
            };
         }
      }


      public static string[] OpenedOrdersHeaders
      {
         get
         {
            return new string[]
            {
               "Label",
               "Amount",
               "Direction",
               "Open price",
               "Profit/Loss at the end",
               "Profit/Loss at the end in pips",
               "Open date",
               "Comment"
            };
         }
      }
   }
}
