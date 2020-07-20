using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarketDataLoader.ExtensionMethods
{
   public static class StringExtensions
   {
      public static DateTime ToDateTime(this string stringToParse)
      {
         DateTime dateTime;
         if (!DateTime.TryParse(stringToParse, out dateTime))
         {
            throw new ArgumentException($"Unable to parse {stringToParse} to date");
         }
         return dateTime;
      }

      public static int ToInteger(this string stringToParse)
      {
         int value;
         if (!int.TryParse(stringToParse, out value))
         {
            throw new ArgumentException($"Unable to parse {stringToParse} to Integer");
         }
         return value;
      }

      public static double ToDouble(this string stringToParse)
      {
         var cultureSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
         stringToParse = stringToParse.Replace(",", cultureSeparator);
         double doubleVal;
         if (!double.TryParse(stringToParse, out doubleVal))
         {
            throw new ArgumentException($"Unable to parse {stringToParse} to double");
         }
         return doubleVal;
      }

      public static string[] Split(this string stringToSplit, string separator)
      {
         return stringToSplit.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
      }
   }
}
