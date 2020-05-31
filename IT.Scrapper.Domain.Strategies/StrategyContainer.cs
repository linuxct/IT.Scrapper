using System;
using System.Collections.Generic;

namespace IT.Scrapper.Domain.Strategies
{
    [Serializable]
    public class StrategyContainer
    {
        public string Url { get; set; }
        public List<Strategy> LinksStrategy { get; set; }
        public List<Strategy> ContentStrategy { get; set; }
        public List<Strategy> TitleStrategy { get; set; }
    }
}