using MarketDataLoader.Converters;
using log4net;
using MarketDataLoader;
using log4net.Repository;
using log4net.Appender;
using System.Linq;
using System.IO;
using System;

namespace MarketDataLoade
{
   class Program
   {
      static void Main(string[] args)
      {
         var htmlFileManager = new HtmlFilesLoadManager(args);
         htmlFileManager.ProcessFiles();
      }
   }
}
