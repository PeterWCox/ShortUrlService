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
    public class UpdateShortUrl
    {
        private readonly IUrlManager _urlManager;

        public UpdateShortUrl(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        [FunctionName(nameof(UpdateShortUrl))]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1.0/Update/{id}")]
        HttpRequest req, ILogger log, string id)
        {
            try
            {
                //Serialize JSON in HTTP request body to get the longUrl the user wishes to generate a shortUrl for.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UrlModel data = JsonConvert.DeserializeObject<UrlModel>(requestBody);
                string longUrl = data?.LongUrl;

                //Return error message if the json body is empty or null.
                if (requestBody == null || string.IsNullOrEmpty(requestBody))
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.RequestBodyEmpty);
                    return new BadRequestObjectResult(errorModel);
                }

                //Return error message if the json body does not contain a 'longUrl' field.
                if (longUrl == null)
                {
                    var errorModel = new ErrorModel(null, null, null, ErrorMessageType.InvalidJson);
                    return new BadRequestObjectResult(errorModel);
                }

                await _urlManager.UpdateUrlModelAsync(int.Parse(id), longUrl);
                UrlModel urlModel = await _urlManager.GetUrlModelByIdAsync(int.Parse(id));

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
            catch (UrlProhibitedException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlProhibited);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlTooLongException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlTooLong);
                return new BadRequestObjectResult(errorModel);
            }                       
            catch (UrlAlreadyExistsException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlAlreadyExists);
                return new BadRequestObjectResult(errorModel);
            }
            catch (UrlNotSyntacticallyCorrectException)
            {
                var errorModel = new ErrorModel(null, null, null, ErrorMessageType.UrlNotSyntacticallyCorrect);
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
