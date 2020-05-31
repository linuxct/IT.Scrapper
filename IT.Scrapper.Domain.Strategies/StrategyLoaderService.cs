using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace IT.Scrapper.Domain.Strategies
{
    public class StrategyLoaderService : IStrategyLoaderService
    {
        public async Task<IEnumerable<string>> GetStrategiesUrls()
        {
            List<StrategyContainer> list;
            await using var fs = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "strategies.json"));
            list = await JsonSerializer.DeserializeAsync<List<StrategyContainer>>(fs, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return list.Select(x => x.Url).ToList();
        }

        public async Task<IEnumerable<IHtmlAnchorElement>> ParseLinksByStrategy(IDocument document)
        {
            var currentStrategy = await ResolveStrategyContent(document);
            return AppliesLinksStrategy(document, currentStrategy);
        }

        public async Task<IEnumerable<string>> ParseContentByStrategy(IDocument document)
        {
            var currentStrategy = await ResolveStrategyContent(document);
            return AppliesContentStrategy(document, currentStrategy);
        }

        public async Task<string> ParseTitleByStrategy(IDocument document)
        {
            var currentStrategy = await ResolveStrategyContent(document);
            return AppliesTitleStrategy(document, currentStrategy);
        }
        
        private static async Task<StrategyContainer> ResolveStrategyContent(IDocument document)
        {
            await using var fs = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "strategies.json"));
            var list = await JsonSerializer.DeserializeAsync<List<StrategyContainer>>(fs, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var currentStrategy = list.First(x => x.Url.Contains(document.Domain));

            return currentStrategy;
        }
        
        private List<IHtmlAnchorElement> AppliesLinksStrategy(IDocument document, StrategyContainer currentStrategy)
        {
            var orderedStrategies = currentStrategy.LinksStrategy.OrderBy(x => x.Order).ToList();
            List<IHtmlAnchorElement> workCollection = new List<IHtmlAnchorElement>();
            foreach (var strategy in orderedStrategies)
            {
                switch (strategy.Type)
                {
                    case StrategyTypeEnum.None:
                        return null;
                    case StrategyTypeEnum.Xpath:
                        workCollection = ApplyXpath<IHtmlAnchorElement>(document, strategy.Value);
                        break;
                    case StrategyTypeEnum.Regex:
                        workCollection = ApplyRegexToLinksCollection(workCollection, strategy.Value);
                        break;
                }
            }

            return workCollection;
        }
        
        private List<string> AppliesContentStrategy(IDocument document, StrategyContainer currentStrategy)
        {
            var orderedStrategies = currentStrategy.ContentStrategy.OrderBy(x => x.Order).ToList();
            List<IElement> workCollection = new List<IElement>();
            List<string> result = new List<string>();
            foreach (var strategy in orderedStrategies)
            {
                switch (strategy.Type)
                {
                    case StrategyTypeEnum.None:
                        return null;
                    case StrategyTypeEnum.Xpath:
                        workCollection = ApplyXpath<IElement>(document, strategy.Value);
                        break;
                    case StrategyTypeEnum.Apply:
                        result = ApplyTextSelector(workCollection, strategy.Value);
                        break;
                }
            }

            result.RemoveAll(string.IsNullOrWhiteSpace);
            return result;
        }
        
        private string AppliesTitleStrategy(IDocument document, StrategyContainer currentStrategy)
        {
            var orderedStrategies = currentStrategy.TitleStrategy.OrderBy(x => x.Order).ToList();
            List<IElement> workCollection = new List<IElement>();
            List<string> result = new List<string>();
            foreach (var strategy in orderedStrategies)
            {
                switch (strategy.Type)
                {
                    case StrategyTypeEnum.None:
                        return null;
                    case StrategyTypeEnum.Xpath:
                        workCollection = ApplyXpath<IElement>(document, strategy.Value);
                        break;
                    case StrategyTypeEnum.Apply:
                        result = ApplyTextSelector(workCollection, strategy.Value);
                        break;
                }
            }
            
            return result.FirstOrDefault();
        }

        private List<string> ApplyTextSelector(List<IElement> workCollection, string strategyValue)
        {
            return strategyValue != "textScope" ? null : workCollection.Select(element => element.TextContent).ToList();
        }

        private List<T> ApplyXpath<T>(IDocument document, string strategyValue) where T : IElement
        {
            var escapedStrategyValue = strategyValue.Replace('\'', '\"');
            return document.QuerySelectorAll<T>($"*[xpath>'{escapedStrategyValue}']").ToList();
        }
        
        private List<IHtmlAnchorElement> ApplyRegexToLinksCollection(List<IHtmlAnchorElement> workCollection, string strategyValue)
        {
            var regex = new Regex(strategyValue);
            return workCollection.Where(link => regex.IsMatch(link.Href)).ToList();
        }
    }
}