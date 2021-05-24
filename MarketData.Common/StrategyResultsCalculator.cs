using System;
using System.Collections.Generic;
using System.Linq;
using MarketData.Common.ExtensionMethods;
using ModelLayer;

namespace MarketData.Common
{
    public class StrategyResultsCalculator
    {
        private SetupInfo _setupInfo;

        private const string InitialDepositId = "Initial deposit";
        private const string FinishDepositId = "Finish deposit";
        private const string CurrencyId = "defaultInstrument";
        private const string PeriodId = "defaultPeriod";

        private List<int> _orderDatesAsNumbers;
        private List<double> _finResPerTrade;

        public StrategyResultsCalculator()
        {
        }

        public StrategyResultsCalculator(SetupInfo setupInfo)
        {
            _setupInfo = setupInfo;
            _orderDatesAsNumbers = new List<int>();
            _finResPerTrade = new List<double>();
        }

        public StrategyResultsDto Calculate(List<HistoricalOrderDto> historicalOrders, List<CalendarLog> calendarLogs, Dictionary<string, string> paramsInfo)
        {
            //количество лет в историческом окне (дата_конец - дата_начало + 1) 
            int yearsInHistoryWindow = (_setupInfo.EndDate - _setupInfo.StartDate.AddDays(1)).Days / 365;
            //позиций_закрыто / (лет_в_историческом_окне * 12)

            double tradesPerMonth = _setupInfo.ClosedPositions / (yearsInHistoryWindow * 12);

            var annualGrowth = CalculateAnnualGrowth(historicalOrders);
            var maximumDrowDown = CalculateMaximumDrowdown(historicalOrders);
            var recovery = CalculateRecovery(annualGrowth, maximumDrowDown);
            var rSquared = CalculateRSquared(calendarLogs);

            var resultsDto = new StrategyResultsDto
            {
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

        public double CalculateRecovery(double annualGrowth, double maximumDrowDown)
        {
            if (annualGrowth <= 0)
            {
                return 0;
            }
            if (maximumDrowDown == 0)
            {
                return 999;
            }
            return Math.Round(annualGrowth / maximumDrowDown, 2);
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
            var backTestMonthes = (_setupInfo.EndDate.Month - _setupInfo.StartDate.Month)
               + 12 * (_setupInfo.EndDate.Year - _setupInfo.StartDate.Year);
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
            return Math.Round(result, 2);
        }

        public double CalculateRSquared(List<CalendarLog> calendarLogs)
        {
            var XminusXAvarage = new List<double>();
            var YminusYAvarage = new List<double>();
            var XYRatio = new List<double>(); // sum of this should be taken as nominator

            foreach (var calendarLog in calendarLogs)
            {
                var calculatedItem = int.Parse(calendarLog.OperationDay.ToString("yyyyMMdd")) -
                                  calendarLogs.Select(day => int.Parse(day.OperationDay.ToString("yyyyMMdd"))).Average();//U8 - AVERAGE($U$8:$U$1926)
                XminusXAvarage.Add(Math.Round(calculatedItem, 0));
            }

            foreach (var calendarLog in calendarLogs)
            {
                var calculatedItem = calendarLog.EodFinRes - calendarLogs.Select(day => day.EodFinRes).Average();//U8 - AVERAGE($U$8:$U$1926)
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

        public double CalculateAnnualGrowth(List<HistoricalOrderDto> historicalOrders)
        {
            if (_setupInfo == null)
            {
                _setupInfo = new SetupInfo();
                _setupInfo.InitialDeposit = 10000;
                _setupInfo.StartDate = new DateTime(2015, 01, 05);
                _setupInfo.EndDate = new DateTime(2020, 06, 07);
            }
            if (!historicalOrders.Any())
            {
                return 0;
            }
            var initialDeposit = _setupInfo.InitialDeposit;
            var newFinishDeposit = historicalOrders.Last().DepositCurve;
            var depositRemainder = (newFinishDeposit - initialDeposit) * 100 / initialDeposit / 100;

            var backTestDays = (_setupInfo.EndDate - _setupInfo.StartDate).Days;

            double powered = (double)365 / (double)backTestDays;

            //средний годовой доход = (1 + чистый) в степени(365 / дней в бэктесте) -1
            var annualGrowth = (Math.Pow(1 + depositRemainder, powered) - 1) * 100;
            return Math.Round(annualGrowth, 2);
        }

        public double CalculateMaximumDrowdown(List<HistoricalOrderDto> historicalOrders)
        {
            if (!historicalOrders.Any())
            {
                return 0;
            }
            double highWaterMark = 0;
            var drawdowns = new List<double>();
            foreach (var trade in historicalOrders)
            {
                highWaterMark = trade.DepositCurve >= highWaterMark ? trade.DepositCurve : highWaterMark;
                //(hwm - кривая_капитала_на_эту_сделку) / hwm
                var drawdown = (highWaterMark - trade.DepositCurve) / highWaterMark;
                drawdowns.Add(drawdown);
            }
            return Math.Round(drawdowns.Max() * 100, 2);
        }
    }
}
