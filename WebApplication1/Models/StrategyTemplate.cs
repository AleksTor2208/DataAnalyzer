using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class StrategyTemplate
    {
        public StrategyTemplate(string name)
        {
            StrategyName = name;
        }

        public string StrategyName { get; set; }
    }
}