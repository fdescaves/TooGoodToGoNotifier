using System;
using System.Net;

namespace TooGoodToGoNotifier.Api
{
    public class TooGoodToGoRequestException : Exception
    {
        public TooGoodToGoRequestException(string message, HttpStatusCode statusCode, string body, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public HttpStatusCode StatusCode { get; }

        public string Body { get; }

        public override string Message => $"{base.Message} | StatusCode: {StatusCode} | Body: {Body}";
    }
}
