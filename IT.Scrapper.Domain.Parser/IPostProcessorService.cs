using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using IT.Scrapper.Domain.Contracts;
using IT.Scrapper.Domain.Strategies;

namespace IT.Scrapper.Domain.Parser
{
    public interface IPostProcessorService
    {
        public Task<List<DownloadedPost>> ProcessPostTask(IEnumerable<string> urls);
    }
}