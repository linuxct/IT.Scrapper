using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace IT.Scrapper.Domain.Strategies
{
    public interface IStrategyLoaderService
    {
        public Task<IEnumerable<string>> GetStrategiesUrls();
        public Task<IEnumerable<IHtmlAnchorElement>> ParseLinksByStrategy(IDocument links);
        public Task<IEnumerable<string>> ParseContentByStrategy(IDocument document);
        public Task<string> ParseTitleByStrategy(IDocument document);
    }
}