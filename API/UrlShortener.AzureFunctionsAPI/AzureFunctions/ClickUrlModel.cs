using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UrlShortener.Core.BusinessLogic;
using UrlShortener.Core.Entities;

namespace UrlShortener.API
{
    public class ClickUrlModel
    {
        private readonly IUrlManager _urlManager;

        public ClickUrlModel(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(ClickUrlModel))]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1.0/Click")]
        HttpRequest req, ILogger log)
        {
            try
            {
                //Serialize JSON in HTTP request body to get the short URL the user has clicked on.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UrlModel data = JsonConvert.DeserializeObject<UrlModel>(requestBody);
                string domainName = data?.DomainName;
                string segment = data?.Segment;


                //Return error message if the json body is empty or null.
                if (requestBody == null || string.IsNullOrEmpty(requestBody))
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.RequestBodyEmpty);
                    return new BadRequestObjectResult(errorModel);
                }

                //Return error message if the json body does not contain a 'longUrl' field.
                if (domainName == null)
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlNotFound);
                    return new BadRequestObjectResult(errorModel);
                }

                await _urlManager.ClickShortUrlAsync(domainName, segment);
                UrlModel urlModel = await _urlManager.GetUrlModelByShortUrlAsync(domainName, segment);

                return new OkObjectResult(urlModel);
            }
            catch (JsonException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.JsonException);
                return new BadRequestObjectResult(errorModel);
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
