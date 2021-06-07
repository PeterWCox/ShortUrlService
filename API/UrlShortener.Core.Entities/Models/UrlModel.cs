using Newtonsoft.Json;
using System;

namespace UrlShortener.Core.Entities
{
    [JsonObject(Title = "urlModel")]
    public class UrlModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("longUrl")]
        public string LongUrl { get; set; }

        [JsonProperty("domainName")]
        public string DomainName { get; set; }

        [JsonProperty("segment")]
        public string Segment { get; set; }

        [JsonProperty("shortUrl")]
        public string ShortUrl => DomainName + "/" + Segment;

        [JsonProperty("added")]
        public DateTime Added { get; set; }

        [JsonProperty("numberOfClicks")]
        public int NumberOfClicks { get; set; }
    }
}
