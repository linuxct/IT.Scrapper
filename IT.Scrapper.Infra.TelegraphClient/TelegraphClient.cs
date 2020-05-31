using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using IT.Scrapper.Domain.Contracts;
using Microsoft.Extensions.Logging;
using Telegraph.Net;
using Telegraph.Net.Models;

namespace IT.Scrapper.Infra.TelegraphClient
{
    public class TelegraphClient : ITelegraphClient
    {
        private readonly Telegraph.Net.TelegraphClient _client;
        private readonly ITokenClient _secureClient;
        private readonly ILogger<TelegraphClient> _logger;
        private readonly IConfiguration _configuration;

        public TelegraphClient(ILogger<TelegraphClient> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new Telegraph.Net.TelegraphClient();
            _secureClient = _client.GetTokenClient(_configuration.GetSection("TelegraphApiKey").Value);
            _logger = logger;
        }
        
        public async Task<bool> CheckIfPageExistsOnTelegraph(string title)
        {
            var validationResult = false;
            var hasResultsPendingToCheck = true;
            var currentOffset = 0;

            try
            {
                while (hasResultsPendingToCheck)
                {
                    var pageList = await _secureClient.GetPageListAsync(offset: currentOffset, limit: 50);
                    if (pageList.Pages.Count == 50)
                        currentOffset += 50;
                    else
                        hasResultsPendingToCheck = false;

                    if (pageList.Pages.Any(page => page.Title == title))
                    {
                        validationResult = true;
                        _logger.LogInformation("Post '{0}' exists on the API", pageList.Pages.Find(x=>x.Title == title)?.Url);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FLOOD_WAIT_"))
                {
                    _logger.LogError("Error while retrieving the posts from the API {0}, waiting 30 seconds.", e.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }
            
            if (!validationResult)
            {
                _logger.LogInformation("Post '{0}' doesn't exist on the API", title);
            }

            return validationResult;
        }

        public async Task PublishPageInTelegraph(DownloadedPost post)
        {
            try
            {
                var result = new List<NodeElement>();
                foreach (var paragraph in post.PostContents)
                {
                    result.Add(paragraph);
                    result.Add(new NodeElement("br", null));
                }
                var operationResult = await _secureClient.CreatePageAsync(post.Title, result.ToArray(), returnContent: true);
                if (!string.IsNullOrWhiteSpace(operationResult.Path))
                {    
                    _logger.LogInformation("Post was successfully saved on the API {0}", operationResult.Url);
                }
                else
                {
                    _logger.LogInformation("Post was not saved on the API {0}", post.Title);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FLOOD_WAIT_"))
                {
                    _logger.LogError("Error while storing in the API {0}, waiting 30 seconds.", e.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }
        }
    }
}