using Newtonsoft.Json;

namespace UrlShortener.Core.Entities
{
    [JsonObject(Title = "error")]
    public class ErrorModel
    {
        public ErrorModel(string id, string longUrl, string shortUrl, ErrorMessageType errorMessageType)
        {
            Id = id;
            LongUrl = longUrl;
            ShortUrl = shortUrl;
            ErrorMessage = GetErrorMessage(errorMessageType);
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("longUrl")]
        public string LongUrl { get; set; }

        [JsonProperty("shortUrl")]
        public string ShortUrl { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        private string GetErrorMessage(ErrorMessageType errorMessageType)
        {
            switch (errorMessageType)
            {
                case ErrorMessageType.RequestBodyEmpty:
                    return "The HTTP request body was null or empty.";
                case ErrorMessageType.InvalidJson:
                    return "JSON contained in the HTTP Request did not contain a longUrl parameter - should be of the form { longUrl: http://google.com, domainName: http://mottmac.com }.";
                case ErrorMessageType.UrlAlreadyExists:
                    return $"A short URL has already been generated for {LongUrl}.";
                case ErrorMessageType.UrlCouldNotBeGenerated:
                    return $"A unique short URL could not be randomly generated for {LongUrl} after 10 attempts - please try again.";
                case ErrorMessageType.UrlNotSyntacticallyCorrect:
                    return $"The long URL {LongUrl} is not syntactically correct - it must begin with http:// or https://";
                case ErrorMessageType.UrlProhibited:
                    return $"A short URL has not been generated for {LongUrl} as it is a prohibited website.";
                case ErrorMessageType.UrlTooLong:
                    return $"The long URL specified is too long - request denied.";
                case ErrorMessageType.IdNotFound:
                    return $"A URL corresponding to an id of {Id} could not be found.";
                case ErrorMessageType.IdNotAValidInteger:
                    return "The Id specified is not a valid number or integer e.g. 3, 5 etc.";
                case ErrorMessageType.NoUrlsFoundException:
                    return "The database contains no URL's.";
                case ErrorMessageType.UrlNotFound:
                    return "A short URL could not be found corresponding to the supplied id.";
                case ErrorMessageType.JsonException:
                    return "The JSON in the request body is not valid - should be in the form of {longUrl: 'http://google.com'}";
                default:
                    return "An unknown error has occurred.";
            }
        }
    }
}
