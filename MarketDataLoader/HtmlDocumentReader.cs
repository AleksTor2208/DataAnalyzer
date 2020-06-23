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
using MarketDataLoader.ExtensionMethods;
using ModelLayer;
using MarketDataLoader.Model;

namespace MarketDataLoader
{
   class HtmlDocumentReader
   {
      private HtmlDocument _doc = new HtmlDocument();
      private HtmlNodeCollection _tables;

      internal List<HistoricalOrderDto> ReadHistoricalOrders()
      {
         var tableWithHistoricalOrders = GetTableWithHistoricOrders(_tables).InnerHtml;
         string cleanedTable = Regex.Replace(tableWithHistoricalOrders, @"\t|\n|\r", "");
         var splittedTable = SplitRows(cleanedTable); 
         var historicalOrders = new List<HistoricalOrderDto>();
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
         _tables = _doc.DocumentNode.SelectNodes("//table");
      }      

      internal List<OrderLog> ReadOrderLogs()
      {
         return OrdersLogConverter.Convert(ReadFromHtmlTable(TableType.OrderLogs));        
      }    

      internal Dictionary<string, string> ReadDetailsTables(TableType tableIndex)
      {
         var resultDictionary = new Dictionary<string, string>();
         foreach (var row in ReadFromHtmlTable(tableIndex))
         {
            var splittedRow = row.Replace("<th>", "").Split("</th>");
            if (!resultDictionary.ContainsKey(splittedRow[0]))
            {
               resultDictionary.Add(splittedRow[0], Regex.Replace(splittedRow[1], @"<td>|</td>", ""));
            }
         }
         return resultDictionary;
      }

      internal void SelectStartEndDate(string htmlFile, ref DateTime backtestStartDate, ref DateTime backtestEndtDate)
      {
         var headerContent = _doc.DocumentNode.SelectNodes("//h1").First().InnerHtml.Split("from");
         var datesAsArray = headerContent[1].Split("to");
         backtestStartDate = datesAsArray[0].ToDateTime();
         backtestEndtDate = datesAsArray[1].ToDateTime();
      }

      internal void LoadFile(string htmlFile)
      {
         _doc.LoadHtml(File.ReadAllText(htmlFile));
      }

      private string[] ReadFromHtmlTable(TableType tableIndex)
      {
         var currentTable = Regex.Replace(_tables[(int)tableIndex].InnerHtml, @"\t|\n|\r", "");
         return SplitRows(currentTable);
      }

      private string[] SplitRows(string table)
      {
         return table.Replace("<tr>", "").Split("</tr>");
      }

      //deprecated
      internal Dictionary<string, string> ReadOrdersInfo()
      {
         var tables = _doc.DocumentNode.SelectNodes("//table");
         var ordersTableInfo = new Dictionary<string, string>();
         int orderInfoTablesIndex = 3;
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
         var currentTable = tables[(int)TableType.OrdersTable];
         if (headers.All(item => currentTable.ChildNodes[1].InnerHtml.Contains(item)))
         {
            return currentTable;
         }
         return null;
      }
   }
}
