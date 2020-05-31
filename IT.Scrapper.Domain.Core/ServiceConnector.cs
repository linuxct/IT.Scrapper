using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IT.Scrapper.Domain.Contracts;
using IT.Scrapper.Domain.Parser;
using IT.Scrapper.Domain.Strategies;
using IT.Scrapper.Infra.TelegraphClient;
using Microsoft.Extensions.Logging;
using Telegraph.Net.Models;

namespace IT.Scrapper.Domain.Core
{
    public class ServiceConnector : IServiceConnector
    {
        private readonly ILogger<ServiceConnector> _logger;
        private readonly IPostProcessorService _postProcessorService;
        private readonly IStrategyLoaderService _strategyLoaderService;
        private readonly ITelegraphClient _telegraphClient;
        
        public ServiceConnector(ILogger<ServiceConnector> logger, IPostProcessorService postProcessorService, IStrategyLoaderService strategyLoaderService,
            ITelegraphClient telegraphClient)
        {
            _logger = logger;
            _postProcessorService = postProcessorService;
            _strategyLoaderService = strategyLoaderService;
            _telegraphClient = telegraphClient;
        }
        
        public async Task FetchPostsTask()
        {
            var urls = await _strategyLoaderService.GetStrategiesUrls();
            var posts = await _postProcessorService.ProcessPostTask(urls);
            foreach (var post in posts)
            {
                if (!await _telegraphClient.CheckIfPageExistsOnTelegraph(post.Title))
                {
                    await _telegraphClient.PublishPageInTelegraph(post);
                }
            }
        }
    }
}