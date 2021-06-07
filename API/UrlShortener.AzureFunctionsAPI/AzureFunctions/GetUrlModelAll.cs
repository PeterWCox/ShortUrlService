using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Core.BusinessLogic;
using UrlShortener.Core.Entities;

namespace UrlShortener.API
{
    public class GetUrlModelAll
    {
        private readonly IUrlManager _urlManager;

        public GetUrlModelAll(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(GetUrlModelAll))]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1.0/Get/All")] HttpRequest req, ILogger log)
        {
            try
            {
                List<UrlModel> urls = await _urlManager.GetUrlModelAllAsync();

                if (urls == null || urls.Count == 0)
                {
                    throw new NoUrlsFoundException();
                }
                else
                {
                    return new OkObjectResult(urls);
                }
            }
            catch (NoUrlsFoundException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.NoUrlsFoundException);
                return new BadRequestObjectResult(errorModel);
            }
            catch (Exception e)
            {
                log.LogCritical($"{0}: {1}", e.GetType(), e.Message);
                log.LogCritical(e.StackTrace);
                return new StatusCodeResult(500);
            }
        }
    }
}
