﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      private const string CurrencyId = "defaultInstrument";
      private const string PeriodId = "defaultPeriod";

      private List<int> _orderDatesAsNumbers;
      private List<double> _finResPerTrade;

      public StrategyResultsDtoConverter(Dictionary<string, string> basicInfo, Dictionary<string, string> paramsInfo, 
                                         Dictionary<string, string> detailsInfo, DateTime backtestStartDate, DateTime backtestEndDate)
      {
         _basicInfo = basicInfo;
         _paramsInfo = paramsInfo;
         _detailsInfo = detailsInfo;
         _backtestStartDate = backtestStartDate;
         _backtestEndDate = backtestEndDate;
         _orderDatesAsNumbers = new List<int>();
         _finResPerTrade = new List<double>();
      }

      internal StrategyResultsDto Convert(List<HistoricalOrderDto> historicalOrders, List<OrderLog> orderLogs, Dictionary<string, string> paramsInfo, long linkNumber)
      {
         var annualGrowth = CalculateAnnualGrowth(historicalOrders);
         var maximumDrowDown = CalculateMaximumDrowdown(historicalOrders, orderLogs);
         var recovery = Math.Round(annualGrowth / maximumDrowDown, 2);
         var rSquared = CalculateRSquared();

         var resultsDto = new StrategyResultsDto
         {
            Id = linkNumber,
            OrdersPerMonth = GetAvarageOrdersQuantityInMonth(historicalOrders.Count()),
            AnualGrowth = annualGrowth,
            MaxDrawDown = maximumDrowDown,
            Recovery = recovery,
            RSquared = rSquared,
            AvarageOrderInPips = CalculateAvarageFinResInPips(historicalOrders),
            StrategyName = historicalOrders.First().Comment,
            Currency = paramsInfo[CurrencyId],
            Timeframe = paramsInfo[PeriodId],//ConvertTimeframe(),
            Parameters = paramsInfo
         };
         return resultsDto;
      }

      private Timeframe ConvertTimeframe(string convertable)
      {
         if (convertable == "4 Hours")
         {
            return Timeframe.FourHours;
         }
         return Timeframe.Daily;
      }

      private double GetAvarageOrdersQuantityInMonth(double ordersQuantity)
      {
         var backTestMonthes = (_backtestEndDate.Month - _backtestStartDate.Month) 
            + 12 * (_backtestEndDate.Year - _backtestStartDate.Year);
         var result = ordersQuantity / backTestMonthes;
         return Math.Round(result, 2);
      }

      private double CalculateAvarageFinResInPips(List<HistoricalOrderDto> historicalOrders)
      {
         var finResultInPips = 0.0;
         foreach (var order in historicalOrders)
         {
            finResultInPips += order.ProfitLossInPips;
         }
         var result = finResultInPips / historicalOrders.Count();
         return Math.Round(result,2);
      }

      private double CalculateRSquared()
      {
         var XminusXAvarage = new List<double>();
         var YminusYAvarage = new List<double>();
         var XYRatio = new List<double>(); // sum of this should be taken as nominator

         foreach (var dateItem in _orderDatesAsNumbers)
         {
            var calculatedItem = dateItem - _orderDatesAsNumbers.Average();//U8 - AVERAGE($U$8:$U$1926)
            XminusXAvarage.Add(Math.Round(calculatedItem, 0));
         }
         foreach (var finResult in _finResPerTrade)
         {
            var calculatedItem = finResult - _finResPerTrade.Average();//U8 - AVERAGE($U$8:$U$1926)
            YminusYAvarage.Add(Math.Round(calculatedItem, 0));
         }

         if (XminusXAvarage.Count() != YminusYAvarage.Count())
         {
            const string errorMessage = "Open order dates and financial results items doesn't match";
            throw new ArgumentException(errorMessage);
         }
         for (int i = 0; i < XminusXAvarage.Count(); i++)
         {
            XYRatio.Add(XminusXAvarage[i] * YminusYAvarage[i]);
         }
         double XYRatioSum = XYRatio.Sum(); // which should be taken as nominator

         double denominator = Math.Sqrt(XminusXAvarage.Select(x => Math.Pow(x, 2)).Sum() *
                              YminusYAvarage.Select(x => Math.Pow(x, 2)).Sum());
         double R = XYRatioSum / denominator;
         double RSquared = Math.Pow(R, 2);
         return Math.Round(RSquared, 2);
      }

      private double CalculateMaximumDrowdown(List<HistoricalOrderDto> historicalOrders, List<OrderLog> orderLogs)
      {
         double initialDeposit = _basicInfo[InitialDepositId].ToDouble();
         double finResult = initialDeposit;
         double highWaterMark = initialDeposit;
         
         //to calculate max drawdown
         var drawdowns = new List<double>();
         
         //to calculate r-squared
         //_finResPerTrade.Add(finResult);
         //var firstDay = orderLogs.OrderBy(log => log.OpenDate).First().OpenDate.AddDays(-1);
         //_orderDatesAsNumbers.Add(int.Parse(firstDay.ToString("yyyyMMdd")));

         foreach (var orderLog in orderLogs.OrderBy(log => log.OpenDate))
         {
            //subtract comission
            if (orderLog.Comisions.Any())
            {
               finResult = finResult - orderLog.Comisions.Sum();
            }

            var historicalOrder = historicalOrders.FirstOrDefault(order => order.Label == orderLog.Label);
            if (historicalOrder != null) //means some order was executed at this day 
            {
               finResult = finResult + historicalOrder.ProfitLoss;
            }
            if (highWaterMark < finResult)
            {
               highWaterMark = finResult;
            }
            
            var drawdown = Math.Round((highWaterMark - finResult) / highWaterMark, 6);
            drawdowns.Add(drawdown);

            //preparing data for R-squared calculations 
            _orderDatesAsNumbers.Add(int.Parse(orderLog.OpenDate.ToString("yyyyMMdd")));
            _finResPerTrade.Add(finResult);
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
