using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketDataLoader.ExtensionMethods;
using ModelLayer;

namespace MarketDataLoader.Converters
{
   class StrategyResultsDtoConverter
   {
      private readonly Dictionary<string, string> _basicInfo;
      private readonly Dictionary<string, string> _paramsInfo;
      private readonly Dictionary<string, string> _detailsInfo;
      private readonly DateTime _backtestStartDate;
      private readonly DateTime _backtestEndDate;

      private const string InitialDepositId = "Initial deposit";
      private const string FinishDepositId = "Finish deposit";

      public StrategyResultsDtoConverter(Dictionary<string, string> basicInfo, Dictionary<string, string> paramsInfo, 
                                         Dictionary<string, string> detailsInfo, DateTime backtestStartDate, DateTime backtestEndDate)
      {
         _basicInfo = basicInfo;
         _paramsInfo = paramsInfo;
         _detailsInfo = detailsInfo;
         _backtestStartDate = backtestStartDate;
         _backtestEndDate = backtestEndDate;
      }

      internal StrategyResultsDto Convert(List<HistoricalOrdersDto> historicalOrders)
      {
         var annualGrowth = CalculateAnnualGrowth(historicalOrders);
         var maximumDrowDown = CalculateMaximumDrowdown(historicalOrders);
         var recovery = -0.0211 / 0.1736;

         var resultsDto = new StrategyResultsDto();
         return resultsDto;
      }

      private double CalculateMaximumDrowdown(List<HistoricalOrdersDto> historicalOrders)
      {
         //var dateRange = new DateRange()

         var initialDeposit = _basicInfo[InitialDepositId].ToDouble();
         var portfolioAmount = initialDeposit;


         var portfolioStateforAllOrders = new List<Tuple<DateTime, double>>();
         foreach (var order in historicalOrders.OrderBy(order => order.OpenDate))
         {
            portfolioAmount = portfolioAmount + order.ProfitLoss;
            portfolioStateforAllOrders.Add(new Tuple<DateTime, double>(order.OpenDate, Math.Round(portfolioAmount, 4)));
         }

         var minPoint = portfolioStateforAllOrders.OrderBy(order => order.Item2).First();
         var maxPoint = portfolioStateforAllOrders.Where(order => order.Item1 < minPoint.Item1)
                                                  .OrderByDescending(order => order.Item2).First();
         //(350 - 750) / 750
         var drawDown = (minPoint.Item2 - maxPoint.Item2) / maxPoint.Item2 * 100;
         var rounded = Math.Round(drawDown, 4);
         return 1.1;
      }

      private double CalculateAnnualGrowth(List<HistoricalOrdersDto> historicalOrders)
      {
         var initialDeposit = _basicInfo[InitialDepositId].ToDouble();
         var finishDeposit = _basicInfo[FinishDepositId].ToDouble();
         var depositRemainder = Math.Round((finishDeposit - initialDeposit) * 100 / initialDeposit / 100, 4);

         var backTestDays = (_backtestEndDate - _backtestStartDate).Days;

         double powered = (double)365 / (double)backTestDays;

         //средний годовой доход = (1 + чистый) в степени(365 / дней в бэктесте) -1
         var annualGrowth = (Math.Pow(1 + depositRemainder, Math.Round(powered, 4)) - 1) * 100;
         return Math.Round(annualGrowth, 2);
      }
   }
}
