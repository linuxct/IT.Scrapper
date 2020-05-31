using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using IT.Scrapper.Domain.Core;
using IT.Scrapper.Domain.Parser;
using IT.Scrapper.Domain.Strategies;
using IT.Scrapper.Infra.TelegraphClient;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IT.Scrapper.WorkerService
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"),
     SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddScoped<IServiceConnector, ServiceConnector>();
                    services.AddScoped<IPostProcessorService, PostProcessorService>();
                    services.AddScoped<IStrategyLoaderService, StrategyLoaderService>();
                    services.AddScoped<ITelegraphClient, TelegraphClient>();
                });
    }
}