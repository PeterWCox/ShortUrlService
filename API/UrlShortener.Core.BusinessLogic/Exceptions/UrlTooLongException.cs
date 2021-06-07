using System;

namespace UrlShortener.Core.BusinessLogic
{
    public class UrlTooLongException : Exception
    {
        public override string Message => "LongUrl specified is too long(> 2048 characters) - request denied.";
    }
}
