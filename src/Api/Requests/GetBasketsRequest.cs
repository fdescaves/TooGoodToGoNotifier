using System.Text.Json.Serialization;
using TooGoodToGoNotifier.Requests;

namespace TooGoodToGoNotifier.Api.Requests
{
    public class GetBasketsRequest
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("origin")]
        public Origin Origin { get; set; }

        [JsonPropertyName("radius")]
        public int Radius { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("favorites_only")]
        public bool FavoritesOnly { get; set; }

        [JsonPropertyName("with_stock_only")]
        public bool WithStockOnly { get; set; }
    }
}