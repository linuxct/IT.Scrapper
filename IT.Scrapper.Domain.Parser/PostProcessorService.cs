using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IT.Scrapper.Domain.Strategies;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using IT.Scrapper.Domain.Contracts;
using Microsoft.Extensions.Logging;

namespace IT.Scrapper.Domain.Parser
{
    public class PostProcessorService : IPostProcessorService
    {
        private readonly ILogger<PostProcessorService> _logger;
        private readonly IStrategyLoaderService _strategyLoaderService;
        
        public PostProcessorService(ILogger<PostProcessorService> logger, IStrategyLoaderService strategyLoaderService)
        {
            _logger = logger;
            _strategyLoaderService = strategyLoaderService;
        }
        
        public async Task<List<DownloadedPost>> ProcessPostTask(IEnumerable<string> urls)
        {
            var angleSharpConfig = Configuration.Default
                .WithCulture("es-es")
                .WithDefaultLoader()
                .WithCss()
                .WithJs()
                .WithXPath();
            var angleSharpContext = BrowsingContext.New(angleSharpConfig);

            var result = new List<DownloadedPost>();
            try
            {
                foreach (var url in urls)
                {
                    _logger.LogInformation("Starts processing posts for URL {0}", url);
                    var mainPageDocument = await angleSharpContext.OpenAsync(url);
                    var links = await _strategyLoaderService.ParseLinksByStrategy(mainPageDocument);
                    var groupedResult = links.GroupBy(x => x.Href);
                    _logger.LogInformation("Got a total of {0} links for URL {1}", groupedResult.Count(), url);
                    foreach (var link in groupedResult)
                    {
                        var downloadedPost = new DownloadedPost();
                        var postDocument = await angleSharpContext.OpenAsync(link.First().Href);
                        var title = await _strategyLoaderService.ParseTitleByStrategy(postDocument);
                        var content = await _strategyLoaderService.ParseContentByStrategy(postDocument);
                        
                        if (string.IsNullOrWhiteSpace(title))
                        {
                            var extractedTitle = string.Empty;
                            foreach (var words in content.ToList().Select(paragraph => paragraph.SplitSpaces()))
                            {
                                extractedTitle = string.Join("", words.Take(7));
                                if (words.Length == 7)
                                {
                                    break;
                                }
                            }

                            title = extractedTitle;
                        }
                        downloadedPost.Title = title;
                        downloadedPost.PostContents = content;
                        
                        result.Add(downloadedPost);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Rest in pepperoni, innerEx: {0}", e.InnerException);
            }

            result.RemoveAll(x => !x.PostContents.Any() || x.PostContents.All(string.IsNullOrWhiteSpace));
            return result;
        }
    }
}