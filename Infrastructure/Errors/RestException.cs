using System;
using System.Net;

namespace conduit_api.Infrastructure.Errors
{
    public class RestException : Exception
    {
        public HttpStatusCode Code { get; }
        public object Errors { get; set; }

        public RestException(HttpStatusCode code, object errors = null)
        {
            Errors = errors;
            Code = code;
        }
    }
}