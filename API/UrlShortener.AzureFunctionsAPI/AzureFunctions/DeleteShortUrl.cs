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
    public class DeleteUrlModel
    {
        private readonly IUrlManager _urlManager;

        public DeleteUrlModel(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(DeleteUrlModel))]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1.0/Delete/{id}")]
        HttpRequest req, ILogger log, string id)
        {
            try
            {
                await _urlManager.DeleteUrlModelAsync(int.Parse(id));
                return new OkObjectResult($"ShortUrl with an id of {id} deleted removed from database.");
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
