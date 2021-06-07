using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.BusinessLogic;
using UrlShortener.DataAccess;

[assembly: FunctionsStartup(typeof(UrlShortener.API.Startup))]
namespace UrlShortener.API
{
    /// <summary>
    /// Configures the dependencies used in the Azure Functions see 
    /// https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    /// </summary>
    public class Startup : FunctionsStartup, IWebJobsStartup
    {
        public Startup()
        {
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IDataConnector, SqlConnector>();
            builder.Services.AddScoped<IUrlManager, UrlManager>();
            builder.Services.AddScoped<IProhibitedSites, ProhibitedSitesMock>();
        }      
    }
}
