using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Core.Entities;

namespace UrlShortener.DataAccess
{
    public interface IDataConnector
    {
        Task CreateUrlModelAsync(UrlModel urlModel);
        Task DeleteUrlModelByIdAsync(int id);
        Task<IEnumerable<UrlModel>> GetUrlModelAllAsync();
        Task<UrlModel> GetUrlModelByIdAsync(int id);
        Task<UrlModel> GetUrlModelByLongUrlAsync(string longUrl);
        Task<UrlModel> GetUrlModelByShortUrlAsync(string domainName, string identifier);
        Task UpdateUrlModelByIdAsync(int id, string longUrl);
        Task ClickShortUrlAsync(string domainName, string identifier);
    }
}