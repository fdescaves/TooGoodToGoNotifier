using System.Collections.Generic;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier
{
    public interface IEmailNotifier
    {
        public void Notify(List<Basket> baskets);
    }
}