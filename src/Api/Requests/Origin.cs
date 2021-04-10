using System.Text.Json.Serialization;

namespace TooGoodToGoNotifier.Requests
{
    public class Origin
    {
        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }
    }
}