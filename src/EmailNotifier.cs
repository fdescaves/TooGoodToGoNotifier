using System;
using System.Collections.Generic;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier
{
    public class EmailNotifier : IEmailNotifier
    {
        public EmailNotifier() { }

        public void Notify(List<Basket> baskets)
        {
            throw new NotImplementedException();
        }
    }
}
