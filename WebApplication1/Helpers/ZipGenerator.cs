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
        internal void GenerateHistoricalOrdersZip(string strategyName, IEnumerable<HistoricalOrders> orders)
        {
            string outputFolder = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData";
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    int count = 1;
                    foreach (var oneSetup in orders)
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

                using (var fileStream = new FileStream($"{outputFolder}\\{strategyName}.zip", FileMode.Open, FileAccess.))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        internal void GenerateStrategyInfoZip(string strategyName, IEnumerable<HistoricalOrders> orders)
        {
            throw new NotImplementedException();
        }
    }
}