using System.Threading.Tasks;
using IT.Scrapper.Domain.Contracts;
using Telegraph.Net.Models;
namespace IT.Scrapper.Infra.TelegraphClient
{
    public interface ITelegraphClient
    {
        public Task<bool> CheckIfPageExistsOnTelegraph(string title);
        public Task PublishPageInTelegraph(DownloadedPost post);
    }
}