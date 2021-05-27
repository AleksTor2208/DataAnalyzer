using System;
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
using WebApplication1.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private IDbConnection _dbConnection;

        public IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    _dbConnection = new MongoDbConnection();
                }
                return _dbConnection;
            }
        }

        public ActionResult Index()
        {
            var strategies = DbConnection.GetOrdersDistinct();
            return View(strategies);
        }

        public async Task<ActionResult> Details(string strategyName)
        {
            var startEndDateTemplate = await DbConnection.GetStartEndDates(strategyName);
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
        public FileResult GenerateReport(string strategyName)
        {
            string outputFolder = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData";

            //FileStreamResult fileStreamResult;
            //var orders = DbConnection.GetOrdersByStrategyName(strategyName);
            //var strategyInfo = DbConnection.GetStrategyInfoByStrategyName(strategyName);

            //var zipGenerator = new ZipGenerator();
            //using (var memoryStream = new MemoryStream())
            //{
            //    zipGenerator.GenerateHistoricalOrdersZip(strategyName, orders, memoryStream);
            //    using (var fileStream = GenerateFileSteam(memoryStream, outputFolder, strategyName))
            //    {
            //        fileStreamResult = File(fileStream, "application/octet-stream", $"{strategyName}.zip");
            //    }
                
            //    //using (var fileStream = new FileStream($"{outputFolder}\\{strategyName}.zip", FileMode.Create))
            //    //{
            //    //    memoryStream.Seek(0, SeekOrigin.Begin);
            //    //    memoryStream.CopyTo(fileStream);

            //    //    memoryStream.Seek(0, SeekOrigin.Begin);
            //    //    memoryStream.CopyTo(outputFileStream);
            //    //    //response = new HttpResponseMessage(HttpStatusCode.OK);
            //    //    //response.Content = new StreamContent(fileStream);//new ByteArrayContent(memoryStream.ToArray());//new StreamContent(fileStream);
            //    //    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            //    //    //response.Content.Headers.ContentDisposition.FileName = $"{strategyName}.zip";
            //    //    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octetstream");
            //    //    //response.Headers.Add("fileName", $"{strategyName}.zip");

            //    //    //System.Web.HttpContext.Current
            //    //    //var request = HttpContext.Current.HttpRequest;
            //    //    //this.HttpContext.ApplicationInstance.Context.Request;

            //    //    //Request.Headers.
            //    //    //long fileSize = memoryStream.Length;
            //    //    //byte[] buffer = new byte[(int)fileSize];
            //    //    //memoryStream.Read(buffer, 0, (int)fileSize);
            //    //    ////memoryStream.Close();
            //    //    //Response.ContentType = "application/zip";
            //    //    //Response.AddHeader("content-disposition", "attachment; filename=" + strategyName);
            //    //    //Response.Write("<b>File Contents: </b>");
            //    //    //Response.BinaryWrite(buffer);
            //    //}
                
            //}
            //ZipFile zipFile = new ZipFile()
            //return fileStreamResult
            byte[] finalResult = System.IO.File.ReadAllBytes($"{outputFolder}\\{strategyName}.zip");
            return File(finalResult, "application/octet-stream", $"{strategyName}.zip");
            //return File(outputFileStream, "application/octet-stream", $"{strategyName}.zip");
            //zipGenerator.GenerateStrategyInfoZip(strategyName, orders);
            //return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public FileResult GenerateReportNew()
        {
            string path = @"C:\Users\alto\source\repos\WebApiDemo\DataAnalyzer\testData\will_sar_xover_v2_mxu.zip";

            //FileStreamResult fileStreamResult;
            //var orders = DbConnection.GetOrdersByStrategyName(strategyName);
            //var strategyInfo = DbConnection.GetStrategyInfoByStrategyName(strategyName);

            //var zipGenerator = new ZipGenerator();
            //using (var memoryStream = new MemoryStream())
            //{
            //    zipGenerator.GenerateHistoricalOrdersZip(strategyName, orders, memoryStream);
            //    using (var fileStream = GenerateFileSteam(memoryStream, outputFolder, strategyName))
            //    {
            //        fileStreamResult = File(fileStream, "application/octet-stream", $"{strategyName}.zip");
            //    }


            //}
            

            byte[] finalResult = System.IO.File.ReadAllBytes(path);
            return File(finalResult, "application/zip", "will_sar_xover_v2_mxu.zip");
          
        }

        private FileStream GenerateFileSteam(MemoryStream memoryStream, string outputFolder, string strategyName)
        {
            var fileStream = new FileStream($"{outputFolder}\\{strategyName}.zip", FileMode.Create);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(fileStream);
            return fileStream;
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