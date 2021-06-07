using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UrlShortener.Core.BusinessLogic;
using UrlShortener.Core.Entities;

namespace UrlShortener.API
{
    public class GetUrlModelById
    {
        private readonly IUrlManager _urlManager;
        public GetUrlModelById(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(GetUrlModelById))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1.0/GetById/{id}")] HttpRequest req, 
            ILogger log, string id)
        {
            try
            {
                UrlModel url = await _urlManager.GetUrlModelByIdAsync(int.Parse(id));
                return new OkObjectResult(url);
            }
            catch (FormatException)
            { 
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.IdNotAValidInteger);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlNotFoundException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlNotFound);
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








