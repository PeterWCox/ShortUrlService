using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Core.Entities;

namespace UrlShortener.Core.BusinessLogic
{
    public interface IUrlManager
    {
        //CRUD Operations
        Task<UrlModel> CreateUrlModelAsync(string longUrl, string domainName);
        Task<UrlModel> GetUrlModelByIdAsync(int id);
        Task<UrlModel> GetUrlModelByShortUrlAsync(string domainName, string identifier);                                             
        Task<UrlModel> GetUrlModelByLongUrlAsync(string longUrl);
        Task<List<UrlModel>> GetUrlModelAllAsync();

        Task UpdateUrlModelAsync(int id, string longUrl);

        Task DeleteUrlModelAsync(int id);

        Task ClickShortUrlAsync(string domainName, string identifier);
    }
}

