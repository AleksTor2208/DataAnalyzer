using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class StartEndDateTemplate
    {
        public StartEndDateTemplate(DateTime start, DateTime end)
        {
            StartDate = start;
            EndDate = end;
        }
        public string StrategyName { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}