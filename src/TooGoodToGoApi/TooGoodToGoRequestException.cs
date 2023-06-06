using System;
using System.Net;

namespace TooGoodToGo.Api
{
    public class TooGoodToGoRequestException : Exception
    {
        public TooGoodToGoRequestException(string message, HttpStatusCode statusCode, string body) : base(message)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public HttpStatusCode StatusCode { get; }

        public string Body { get; }

        public override string Message => $"{base.Message} | StatusCode: {StatusCode} | Body: {Body}";
    }
}
