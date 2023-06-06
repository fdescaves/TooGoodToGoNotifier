namespace TooGoodToGoNotifier.Core.Options
{
    public class TooGoodToGoApiOptions
    {
        public string BaseUrl { get; set; }

        public string AuthenticateByEmailEndpoint { get; set; }

        public string AuthenticateByRequestPollingIdEndpoint { get; set; }

        public string RefreshTokenEndpoint { get; set; }

        public string GetItemsEndpoint { get; set; }

        public string AccountEmail { get; set; }
    }
}
