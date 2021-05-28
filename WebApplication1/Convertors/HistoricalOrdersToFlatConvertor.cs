using ModelLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Convertors
{
    public class HistoricalOrdersToFlatConvertor
    {
        internal IEnumerable<HistoricalOrdersFlat> Convert(string strategyName, HistoricalOrders oneSetup)
        {
            var orders = oneSetup.Orders;
            var parameters = oneSetup.Parameters;
            var strategyResults = oneSetup.StrategyResults;

            var flatOrders = new List<HistoricalOrdersFlat>();
            bool isFirstRow = true;
            foreach (var tradeRow in orders)
            {
                var flatOrder = new HistoricalOrdersFlat()
                {
                    Label = tradeRow.Label,
                    Amount = tradeRow.Amount,
                    Direction = tradeRow.Direction,
                    OpenPrice = tradeRow.OpenPrice,
                    ClosePrice = tradeRow.ClosePrice,
                    ProfitLoss = tradeRow.ProfitLoss,
                    ProfitLossInPips = tradeRow.ProfitLossInPips,
                    OpenDate = tradeRow.OpenDate,
                    CloseDate = tradeRow.CloseDate,
                    AdjustedOpenDate = tradeRow.AdjustedOpenDate,
                    AdjustedCloseDate = tradeRow.AdjustedCloseDate,
                    DepositCurve = tradeRow.DepositCurve,
                    PercentChange = tradeRow.PercentChange,
                    FirstCommission = tradeRow.Commissions != null ? tradeRow.Commissions.FirstOrDefault() : 0,
                    SecondCommission = tradeRow.Commissions != null && tradeRow.Commissions.Count() > 1 ? tradeRow.Commissions[1] : 0
                    //StrategyResults
                };

                if (isFirstRow)
                {
                    flatOrder.StrategyName = strategyName;
                    flatOrder.Parameters = JsonConvert.SerializeObject(parameters);
                    flatOrder.OrdersPerMonth = strategyResults.OrdersPerMonth.ToString();
                    //Strategy results
                    flatOrder.AnualGrowth = strategyResults.AnualGrowth.ToString();
                    flatOrder.MaxDrawDown = strategyResults.MaxDrawDown.ToString();
                    flatOrder.Recovery = strategyResults.Recovery.ToString();
                    flatOrder.RSquared = strategyResults.RSquared.ToString();
                    flatOrder.AvarageOrderInPips = strategyResults.AvarageOrderInPips.ToString();
                    flatOrder.Currency = strategyResults.Currency;
                    flatOrder.Timeframe = strategyResults.Timeframe;

                    isFirstRow = false;
                }
                flatOrders.Add(flatOrder);
            }
            return flatOrders;
        }
    }
}