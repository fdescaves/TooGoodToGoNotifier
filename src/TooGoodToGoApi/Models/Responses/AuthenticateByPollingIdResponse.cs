namespace TooGoodToGo.Api.Models.Responses
{
    public class AuthenticateByPollingIdResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public TgtgStartupData StartupData { get; set; }
    }
}