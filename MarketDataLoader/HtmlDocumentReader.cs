using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarketDataLoader.Converters;

namespace MarketDataLoader
{
   class HtmlDocumentReader
   {
      private HtmlDocument _doc = new HtmlDocument();
      private HtmlNodeCollection _tables;

      internal List<HistoricalOrdersDto> ReadHistoricalOrders()
      {
         var tableWithHistoricalOrders = GetTableWithHistoricOrders(_tables).InnerHtml;
         string cleanedTable = Regex.Replace(tableWithHistoricalOrders, @"\t|\n|\r", "");
         var splittedTable = SplitRows(cleanedTable); 
         var historicalOrders = new List<HistoricalOrdersDto>();
         var historicalOrderConverter = new HistoricalOrdersConverter();
         foreach (var row in splittedTable)
         {
            if (row.Contains("<th>"))
               continue;
            historicalOrders.Add(historicalOrderConverter.Convert(row));
         }
         return historicalOrders;
      }

      internal void SelectTablesFromFile(string htmlFile)
      {
         _doc.LoadHtml(File.ReadAllText(htmlFile));
         _tables = _doc.DocumentNode.SelectNodes("//table");
      }

      internal Dictionary<string, string> ReadFromHtmlTable(TableType tableIndex)
      {
         var resultDictionary = new Dictionary<string, string>();
         var currentTable = Regex.Replace(_tables[(int)tableIndex].InnerHtml, @"\t|\n|\r", "");
         var splittedTable = SplitRows(currentTable);
         foreach (var row in splittedTable)
         {
            var splittedRow = row.Replace("<th>", "").Split(
             new[] { "</th>" },
             StringSplitOptions.RemoveEmptyEntries);
            if (!resultDictionary.ContainsKey(splittedRow[0]))
            {
               resultDictionary.Add(splittedRow[0], Regex.Replace(splittedRow[1], @"<td>|</td>", ""));
            }
         }
         return resultDictionary;
      }

      private string[] SplitRows(string table)
      {
         return table.Replace("<tr>", "").Split(
                new[] { "</tr>" },
                StringSplitOptions.RemoveEmptyEntries);
      }

      //deprecated
      internal Dictionary<string, string> ReadOrdersInfo()
      {
         var tables = _doc.DocumentNode.SelectNodes("//table");
         var ordersTableInfo = new Dictionary<string, string>();
         int orderInfoTablesIndex = 3;
         int parameterIndex = 1;
         for (int i = 0; i < orderInfoTablesIndex; i++)
         {
            var currentTable = Regex.Replace(tables[i].InnerHtml, @"\t|\n|\r", "");
            var splittedTable = SplitRows(currentTable);
            foreach (var row in splittedTable)
            {
               var splittedRow = row.Replace("<th>", "").Split(
                new[] { "</th>" },
                StringSplitOptions.RemoveEmptyEntries);
               if (!ordersTableInfo.ContainsKey(splittedRow[0]))
               {
                  ordersTableInfo.Add(splittedRow[0], Regex.Replace(splittedRow[1], @"<td>|</td>", ""));
               }
            }
         }
         return ordersTableInfo;
      }

      private HtmlNode GetTableWithHistoricOrders(HtmlNodeCollection tables)
      {
         string[] headers = { "Label", "Amount", "Direction", "Open price", "Close price",
                              "Profit/Loss", "Profit/Loss in pips", "Open date", "Close date", "Comment" };
         const int OrdersTableIndex = 4;
         var currentTable = tables[OrdersTableIndex];
         if (headers.All(item => currentTable.ChildNodes[1].InnerHtml.Contains(item)))
         {
            return currentTable;
         }
         return null;
      }
   }
}
