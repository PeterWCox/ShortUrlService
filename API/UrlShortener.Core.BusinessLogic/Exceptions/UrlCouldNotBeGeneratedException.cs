using System;

namespace UrlShortener.Core.BusinessLogic
{
    public class UrlCouldNotBeGeneratedException : Exception
    {
        public override string Message =>
            "A unique short url could not be generated after a given number attempts - please try again.";

    }
}
