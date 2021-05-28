using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class HistoricalOrdersFlat
    {
        public string StrategyName { get; set; }

        public string Label { get; set; }

        public string Amount { get; set; }

        public string Direction { get; set; }

        public double OpenPrice { get; set; }

        public double ClosePrice { get; set; }

        public double ProfitLoss { get; set; }

        public double ProfitLossInPips { get; set; }

        public DateTime OpenDate { get; set; }

        public DateTime CloseDate { get; set; }

        public DateTime AdjustedOpenDate { get; set; }

        public DateTime AdjustedCloseDate { get; set; }

        public double DepositCurve { get; set; }

        public double PercentChange { get; set; }

        public double FirstCommission { get; set; }

        public double SecondCommission { get; set; }

        public string Parameters { get; set; }

        //Strategy Results
        public string OrdersPerMonth { get; set; }
        public string AnualGrowth { get; set; }
        public string MaxDrawDown { get; set; }
        public string Recovery { get; set; }
        public string RSquared { get; set; }
        public string AvarageOrderInPips { get; set; }
        public string Currency { get; set; }
        public string Timeframe { get; set; }
    }
}