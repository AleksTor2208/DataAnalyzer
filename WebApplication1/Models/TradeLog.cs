using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class TradeLog
    {
        public string Direction { get; set; }
        public string OpenPrice { get; set; }
        public string ClosePrice { get; set; }
        public string ProfitLoss { get; set; }
        public string ProfitLossInPips { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }
        public double FirstCommission { get; set; }
        public double SecondCommission { get; set; }
        public double DepositCurve { get; set; }
        public double PercentChange { get; set; }
        public DateTime AdjustedOpenDate { get; set; }
        public DateTime AdjustedCloseDate { get; set; }


    }
}