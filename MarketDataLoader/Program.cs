using MarketDataLoader.Converters;

namespace MarketDataLoader
{
   class Program
   {
      static void Main(string[] args)
      {
         string inputHtml = @"C:\Users\ASUS\Documents\FX\sample_reports\usdjpy\jforex_optimizer28848197175903562.html";
         var htmlReader = new HtmlDocumentReader(inputHtml);

         //var historicalOrders = htmlReader.ReadHistoricalOrders(inputHtml);
         //var bsonDocuments = BSonConverter.GenerateBSonDocuments(historicalOrders);

         var accountInfo = htmlReader.ReadFromHtmlTable(TableType.AccountInfo);
         var paramsInfo = htmlReader.ReadFromHtmlTable(TableType.ParamsInfo);
         var detailsInfo = htmlReader.ReadFromHtmlTable(TableType.DetailsInfo);

         var ordersInfo = BSonConverter.GenerateOrdersInfoDocument(accountInfo, paramsInfo, detailsInfo);

         //var OrdersInfo = htmlReader.ReadOrdersInfo();

         var dbConnection = new DbConnection();
         dbConnection.LoadOrdersInfo(ordersInfo);
         //dbConnection.LoadOrders(bsonDocuments);
      }

      
   }
}
