using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvHelper.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public class HistoricalOrdersMap : ClassMap<HistoricalOrdersFlat>
    {
        public HistoricalOrdersMap()
        {
            Map(m => m.StrategyName).Name("StrategyName");
            Map(m => m.Direction).Name("Direction");
            Map(m => m.OpenPrice).Name("OpenPrice");
            Map(m => m.ClosePrice).Name("ClosePrice");
            Map(m => m.ProfitLoss).Name("Profit/Loss");
            Map(m => m.ProfitLossInPips).Name("Profit/Loss in pips");
            Map(m => m.OpenDate).Name("Open Date");
            Map(m => m.CloseDate).Name("Close Date");
            Map(m => m.FirstCommission).Name("First Commission");
            Map(m => m.SecondCommission).Name("Second Commission");
            Map(m => m.DepositCurve).Name("Deposit Curve");
            Map(m => m.PercentChange).Name("Percent Change");
            Map(m => m.AdjustedOpenDate).Name("Adjusted Open Date");
            Map(m => m.AdjustedCloseDate).Name("Adjusted Close Date");
            Map(m => m.Parameters).Name("Parameters");

            //Strategy Results mapping
            Map(m => m.OrdersPerMonth).Name("OrdersPerMonth");
            Map(m => m.AnualGrowth).Name("AnualGrowth");
            Map(m => m.MaxDrawDown).Name("MaxDrawDown");
            Map(m => m.Recovery).Name("Recovery");
            Map(m => m.RSquared).Name("RSquared");
            Map(m => m.AvarageOrderInPips).Name("AvarageOrderInPips");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.Timeframe).Name("Timeframe");
            
    }
    }
}