using ModelLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.Helpers
{
    class TradeLogToHistoryOrders
    {
        private List<TradeLogDto> tradeLogs;

        public TradeLogToHistoryOrders(List<TradeLogDto> tradeLog)
        {
            this.tradeLogs = tradeLog;
        }

        internal HistoricalOrders Convert()
        {
            var orders = new List<HistoricalOrderDto>();
            foreach (var tradeLog in tradeLogs)
            {
                var historicalOrder = new HistoricalOrderDto()
                {
                    Direction = tradeLog.Direction,
                    OpenPrice = Double.Parse(tradeLog.OpenPrice),
                    ClosePrice = Double.Parse(tradeLog.ClosePrice),
                    ProfitLoss = Double.Parse(tradeLog.ProfitLoss),
                    ProfitLossInPips = double.Parse(tradeLog.ProfitLossInPips),
                    OpenDate = DateTime.Parse(tradeLog.OpenDate),
                    CloseDate = DateTime.Parse(tradeLog.CloseDate),
                    DepositCurve = double.Parse(tradeLog.DepositCurve),
                    AdjustedOpenDate = DateTime.Parse(tradeLog.AdjustedOpenDate),
                    AdjustedCloseDate = DateTime.Parse(tradeLog.AdjustedCloseDate)
                };
                orders.Add(historicalOrder);
            }
            return new HistoricalOrders(orders);
        }
    }
}
