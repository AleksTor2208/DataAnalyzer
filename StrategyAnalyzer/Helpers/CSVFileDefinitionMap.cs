using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StrategyAnalyzer.Helpers
{
    public class CSVFileDefinitionMap : ClassMap<TradeLogDto>
    {
        public CSVFileDefinitionMap()
        {
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

        }
    }
}