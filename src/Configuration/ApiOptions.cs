namespace TooGoodToGoNotifier.Configuration
{
    public class ApiOptions
    {
        public string BaseUrl { get; set; }

        public string AuthenticateEndpoint { get; set; }

        public string RefreshTokenEndpoint { get; set; }

        public string GetItemsEndpoint { get; set; }

        public int RefreshTokenInterval { get; set; }

        public AuthenticationOptions AuthenticationOptions { get; set; }
    }
}
