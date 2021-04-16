using System;

namespace TooGoodToGoNotifier.Api
{
    public class TooGoodToGoRequestException : Exception
    {
        public TooGoodToGoRequestException(string message) : base(message) { }
    }
}
