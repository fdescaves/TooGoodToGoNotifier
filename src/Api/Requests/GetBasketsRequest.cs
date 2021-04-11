using System.Text.Json.Serialization;
using TooGoodToGoNotifier.Requests;

namespace TooGoodToGoNotifier.Api.Requests
{
    public class GetBasketsRequest
    {
        public int UserId { get; set; }

        public Origin Origin { get; set; }

        public int Radius { get; set; }

        public int PageSize { get; set; }

        public int Page { get; set; }

        public bool FavoritesOnly { get; set; }

        public bool WithStockOnly { get; set; }
    }
}