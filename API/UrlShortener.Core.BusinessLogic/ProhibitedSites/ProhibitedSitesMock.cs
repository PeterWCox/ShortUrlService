using System.Collections.Generic;

namespace UrlShortener.Core.BusinessLogic
{
    public class ProhibitedSitesMock : IProhibitedSites
    {
        public bool VerifyIfLongUrlIsProhibited(string longUrl)
        {
            return ProhibitedSites().Contains(longUrl);
        }

        private List<string> ProhibitedSites()
        {
            return new List<string>()
            {
                "http://bing.com",
                "https://bing.com",
                "http://java.com",
                "https://java.com",
            };
        }



    }
}
