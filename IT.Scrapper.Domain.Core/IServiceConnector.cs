using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.Scrapper.Domain.Core
{
    public interface IServiceConnector
    {
        public Task FetchPostsTask();
    }
}