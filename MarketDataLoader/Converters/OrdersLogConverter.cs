using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarketDataLoader.ExtensionMethods;
using MarketDataLoader.Model;

namespace MarketDataLoader.Converters
{
   class OrdersLogConverter
   {
      internal static IEnumerable<OrderLog> Convert(string[] htmlLogs)
      {
         var orderLogs = new List<OrderLog>();

         for (int i = 1; i < htmlLogs.Length; i++)
         {
            var orderLog = new OrderLog();
            var logsArray = htmlLogs[i].Replace("<td>", "").Split("</td>");

            if (logsArray[1] == "Commissions")
            {
               double comission = GetComission(logsArray);
               if (comission != 0)
               {
                  orderLog.Comisions.Add(comission);
                  orderLog.OperationDay = logsArray[0].ToDateTime();
                  orderLogs.Add(orderLog);
               }
            }
         }
         return orderLogs.GroupBy(log => log.OperationDay).SelectMany(log => log).ToList();
      }


      internal static List<OrderLog> Convert_bkp(string[] htmlLogs)
      {
         var orderLogs = new List<OrderLog>();

         bool isNewOrder = true;
         bool isFirstOrder = true;
         var orderLog = new OrderLog();

         for (int i = 1; i < htmlLogs.Length; i++)
         {
            var logsArray = htmlLogs[i].Replace("<td>", "").Split("</td>");


            //if Order filled, to sozdaju novyj OrderLog i poka ne budet Order submitted
            // pishu vse komissii v etot objekt, kogda submitted, 
            if (logsArray[1] == "Order filled")
            {
               isNewOrder = false;
               orderLog.OperationDay = logsArray[0].ToDateTime();
               //Order[IVF20150128_03595926306857, USD / JPY, BUY, 20414.0 at 118.144] filled
               var currentLog = logsArray[2].Replace(" ", string.Empty);
               Regex pattern = new Regex(@".+\[(.+)\,.+\].+");
               Match match = pattern.Match(logsArray[2]);
               var label = match.Groups[1].Value.Split(',').First();
               orderLog.Label = label;
            }

            if (!isNewOrder)
            {
               if (logsArray[1] == "Commissions")
               {
                  double comission = GetComission(logsArray);                  
                  if (comission != 0)
                  {
                     orderLog.Comisions.Add(comission);
                  }
               }
            }
            if (logsArray[1] == "Order submitted" || i == htmlLogs.Length - 1) // or this is last item
            {

               isNewOrder = true;
               if (isFirstOrder)
               {
                  isFirstOrder = false;
               }
               else
               {
                  orderLogs.Add(orderLog);
               }
               
               orderLog = new OrderLog();
            }
         }
         var strBuilder = new StringBuilder();
         foreach (var log in orderLogs)
         {
            if (log.Comisions.Count == 0)
            {
               strBuilder.Append($"{log.OperationDay},{log.Label},0\n");
            }
            if (log.Comisions.Count == 1)
            {
               strBuilder.Append($"{log.OperationDay},{log.Label},{log.Comisions.First()}\n");
            }
            if (log.Comisions.Count == 2)
            {
               strBuilder.Append($"{log.OperationDay},{log.Label},{log.Comisions.First()},{log.Comisions[1]}\n");
            }
         }
         string path = @"C:\Users\ASUS\Documents\FX\sample_reports\ordersWithCommissions.csv";
         File.WriteAllText(path, strBuilder.ToString());

         return orderLogs;
      }

      private static double GetComission(string[] logsArray)
      {
         Regex pattern = new Regex(@".+\s\[(.+)\]");
         Match match = pattern.Match(logsArray[2]);
         if (match.Success)
         {
            return match.Groups[1].Value.ToDouble();
         }
         return 0.0;
      }
   }
}
