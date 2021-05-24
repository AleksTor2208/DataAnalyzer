using CsvHelper.Configuration.Attributes;

namespace StrategyAnalyzer.Helpers
{
    public class TradeLogDto
    {
        [Name("Direction")]
        public string Direction { get; set; }

        [Name(" OpenPrice")]
        public string OpenPrice { get; set; }

        [Name(" ClosePrice")]
        public string ClosePrice { get; set; }

        [Name(" Profit/Loss")]
        public string ProfitLoss { get; set; }

        [Name(" Profit/Loss in pips")]
        public string ProfitLossInPips { get; set; }

        [Name(" Open Date")]
        public string OpenDate { get; set; }

        [Name(" Close Date")]
        public string CloseDate { get; set; }

        [Name(" First Commission")]
        public string FirstCommission { get; set; }

        [Name(" Second Commission")]    
        public string SecondCommission { get; set; }

        [Name(" Deposit Curve")]
        public string DepositCurve { get; set; }

        [Name(" Percent Change")]
        public string PercentChange { get; set; }

        [Name(" Adjusted Open Date")]
        public string AdjustedOpenDate { get; set; }

        [Name(" Adjusted Close Date")]
        public string AdjustedCloseDate { get; set; }
    }
}