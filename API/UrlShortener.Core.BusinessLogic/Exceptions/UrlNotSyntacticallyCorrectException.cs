using System;

namespace UrlShortener.Core.BusinessLogic
{
    public class UrlNotSyntacticallyCorrectException : Exception
    {
        public override string Message =>
          "The long url specified is a valid url syntactically e.g. => google...com htp://google.com";

    }
}
