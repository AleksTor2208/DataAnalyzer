using CsvHelper;
using ModelLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using WebApplication1.Convertors;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public class ZipGenerator
    {
        internal void GenerateHistoricalOrdersZip(string strategyName, IEnumerable<HistoricalOrders> tradeLogs, MemoryStream memoryStream)
        {
            //string outputFolder = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData";
            
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                int count = 1;
                foreach (var oneSetup in tradeLogs)
                {
                    var fileName = $"TradeLogs_{strategyName}_setup{count}.csv";
                    var flatOrders = new HistoricalOrdersToFlatConvertor().Convert(strategyName, oneSetup);
                    var demoFile = archive.CreateEntry($"{fileName}");

                    using (var entryStream = demoFile.Open())
                    using (StreamWriter writer = new StreamWriter(entryStream))
                    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.RegisterClassMap<HistoricalOrdersMap>();
                        {
                            csv.WriteRecords(flatOrders);
                        }
                    }
                    count++;
                }
            }
        }

        internal void GenerateStrategyInfoZip(string strategyName, IEnumerable<HistoricalOrders> orders)
        {
            throw new NotImplementedException();
        }
    }
}