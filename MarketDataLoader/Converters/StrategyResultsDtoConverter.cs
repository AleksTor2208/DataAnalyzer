using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketDataLoader.ExtensionMethods;
using MarketDataLoader.Model;
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

      internal StrategyResultsDto Convert(List<HistoricalOrderDto> historicalOrders, List<OrderLog> orderLogs)
      {
         var annualGrowth = CalculateAnnualGrowth(historicalOrders);
         var maximumDrowDown = CalculateMaximumDrowdown(historicalOrders, orderLogs);
         var recovery = annualGrowth / maximumDrowDown;


         var resultsDto = new StrategyResultsDto();
         return resultsDto;
      }

      private double CalculateMaximumDrowdown(List<HistoricalOrderDto> historicalOrders, List<OrderLog> orderLogs)
      {
         double initialDeposit = _basicInfo[InitialDepositId].ToDouble();
         double portfolioAmount = initialDeposit;

         double highWaterMark = initialDeposit;
         var drawdowns = new List<double>();
         foreach (var orderLog in orderLogs.OrderBy(log => log.OpenDate))
         {
            //subtract comission
            if (orderLog.Comisions.Any())
            {
               portfolioAmount = portfolioAmount - orderLog.Comisions.Sum();
            }

            var historicalOrder = historicalOrders.FirstOrDefault(order => order.Label == orderLog.Label);
            //DateTime.Compare(order.OpenDate.Date, orderLog.OpenDate.Date) == 0);
            if (historicalOrder != null) //means some order was executed at this day 
            {
               portfolioAmount = portfolioAmount + historicalOrder.ProfitLoss;
            }
            if (highWaterMark < portfolioAmount)
            {
               highWaterMark = portfolioAmount;
            }
            
            var drawdown = Math.Round((highWaterMark - portfolioAmount) / highWaterMark, 6);
            drawdowns.Add(drawdown);
         }
         return Math.Round(drawdowns.Max() * 100, 2);
      }

      private double CalculateAnnualGrowth(List<HistoricalOrderDto> historicalOrders)
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
