using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System;
using System.Threading.Tasks;
using UrlShortener.Core.BusinessLogic;
using UrlShortener.Core.Entities;

namespace UrlShortener.API
{
    public class CreateUrlModel
    {
        private readonly IUrlManager _urlManager;

        public CreateUrlModel(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(CreateUrlModel))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1.0/Create")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                //Serialize JSON in HTTP request body to get the URL the user wishes to generate a shortened URL for.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UrlModel data = JsonConvert.DeserializeObject<UrlModel>(requestBody);
                string longUrl = data?.LongUrl;
                string domainName = data?.DomainName;

                //Return error message if the JSON body is empty or null.
                if (requestBody == null || string.IsNullOrEmpty(requestBody))
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.RequestBodyEmpty);
                    return new BadRequestObjectResult(errorModel);
                }

                //Return error message if the JSON body does not contain a 'longUrl' field.
                if (longUrl == null || domainName == null)
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.InvalidJson);
                    return new BadRequestObjectResult(errorModel);
                }

                UrlModel urlModel = await _urlManager.CreateUrlModelAsync(longUrl, domainName);
                return new OkObjectResult(urlModel);
            }
            catch (JsonException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.JsonException);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlTooLongException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlTooLong);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlProhibitedException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlProhibited);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlNotSyntacticallyCorrectException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlNotSyntacticallyCorrect);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlCouldNotBeGeneratedException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlCouldNotBeGenerated);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlAlreadyExistsException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlAlreadyExists);
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
