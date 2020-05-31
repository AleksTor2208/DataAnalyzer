﻿using ModelLayer;
using System.Collections.Generic;

namespace StrategyAnalyzer.DAL
{
   public interface IDbConnection
   {
      IEnumerable<StrategyResultsDto> GetStrategyInfo(string strategyName, string currency, string hour);
   }
}