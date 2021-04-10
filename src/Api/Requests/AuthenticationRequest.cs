using System.Text.Json.Serialization;

namespace TooGoodToGoNotifier.Api.Requests
{
    public class AuthenticationRequest
    {
        [JsonPropertyName("device_type")]
        public string DeviceType { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
