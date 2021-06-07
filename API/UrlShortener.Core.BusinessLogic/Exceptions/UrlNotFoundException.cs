using System;

namespace UrlShortener.Core.BusinessLogic
{
    public class UrlNotFoundException : Exception
    {
        public override string Message =>
          "The Url specified could not be found.";

    }
}
