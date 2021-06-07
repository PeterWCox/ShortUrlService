using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.API
{
    public static class HTTPHelpers
    {
        public static string GetIPAddress(HttpRequest req)
        {
            return req.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }

}
