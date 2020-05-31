using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IT.Scrapper.Domain.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IT.Scrapper.WorkerService
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var serviceConnector = scope.ServiceProvider.GetRequiredService<IServiceConnector>();
                    await serviceConnector.FetchPostsTask();
                }
                
                _logger.LogInformation("Finished processing posts");
                
                double.TryParse(_configuration["TimeToRefreshInHours"], out var timeToRefreshInHours);
                var timeToRefreshInMillis = Convert.ToInt32(timeToRefreshInHours * 60 * 60 * 1000);
                await Task.Delay(timeToRefreshInMillis, stoppingToken);
            }
        }
    }
}