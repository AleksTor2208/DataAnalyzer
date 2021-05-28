using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
    [BsonIgnoreExtraElements]
    public class HistoricalOrders
    {
        public HistoricalOrders()
        { }

        public HistoricalOrders(IEnumerable<HistoricalOrderDto> orders)
        {
            Orders = orders;
        }
        public string StrategyName { get; set; }
        public StrategyResultsDto StrategyResults { get; set; }
        public IEnumerable<HistoricalOrderDto> Orders { get; private set; }
        public IDictionary<string, string> Parameters { get; set; }
    }
}
