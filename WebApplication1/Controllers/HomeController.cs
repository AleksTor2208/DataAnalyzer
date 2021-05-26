﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.IO;
using System.Globalization;
using WebApplication1.Helpers;
using CsvHelper;
using System.Text;
using StrategyAnalyzer.Controllers;
using System.IO.Compression;
using System.Threading.Tasks;
using System.IO;
using MarketDataLoader;
using WebApplication1.DAL;
using System.Net;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private IDataProvider _dbProvider;

        public IDataProvider DbProvider
        {
            get
            {
                if (_dbProvider == null)
                {
                    _dbProvider = new OrdersDataProvider(new MongoDbConnection());
                }
                return _dbProvider;
            }
        }

        public ActionResult Index()
        {
            var strategies = DbProvider.GetStrategiesDistinct();
            return View(strategies);
        }

        public async Task<ActionResult> Details(string strategyName)
        {
            var startEndDateTemplate = await DbProvider.GetStartEndDates(strategyName);
            startEndDateTemplate.StrategyName = strategyName;
            return View(new List<StartEndDateTemplate>() { startEndDateTemplate });
        }

        private List<TradeLogDto> ReadData(string path)
        {
            List<TradeLogDto> tradeLogs = new List<TradeLogDto>();
            if (System.IO.File.Exists(path))
            {
                FileStream fileStream = System.IO.File.OpenRead(path);
                using (var reader = new StreamReader(fileStream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<CSVFileDefinitionMap>();
                    while (csv.Read())
                    {
                        //var record = csv.GetRecord<TradeLogDto>();

                        //csv.Configuration.HasHeaderRecord = true;
                        //csv.Configuration.MissingFieldFound.
                        var record = csv.GetRecord<TradeLogDto>();
                        tradeLogs.Add(record);
                    }
                }
            }
            return tradeLogs;
        }

        public ActionResult TradeLog()
        {
            //var extension = "*.csv";
            //var folderPath = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData\csv";
            //foreach (var file in Directory.EnumerateFiles(folderPath, extension))
            //{

            //}
            var filePath = @"C:\Users\alto\source\repos\WebApiDemo\4_5992517574594660734.csv";
            var data = ReadData(filePath);
            return View(data);           
        }

        [HttpGet]
        public ActionResult Validate()
        {
            string startegy = Request["sname"];
            string currency = Request["currency"];
            string timeFrame = Request["timeframe"];
            DateTime startDate = DateTime.Parse(Request["startDate"]);
            DateTime endDate = DateTime.Parse(Request["endDate"]);
            string trainPeriod = Request["trainPeriod"];
            string validationPeriod = Request["validationPeriod"];

            var strategyAnalyzer = new StrategyExprorerController(null);
            strategyAnalyzer.GetValidationInfo(startegy, currency, timeFrame, trainPeriod, validationPeriod, startDate, endDate);
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
              
        public ActionResult Import()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult GenerateReport(string strategyName)
        {
            var orders = DbProvider.GetOrdersByStrategyName(strategyName);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult ProcessImport(HttpPostedFileBase file)
        {
            ViewBag.Message = "Your contact page.";

            ZipArchive archive = new ZipArchive(file.InputStream);
            var htmlFileManager = new HtmlFilesLoadManager();
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = entry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var htmlDocument = reader.ReadToEnd();
                        htmlFileManager.ProcessFile(entry.ToString(), htmlDocument);
                    }
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}