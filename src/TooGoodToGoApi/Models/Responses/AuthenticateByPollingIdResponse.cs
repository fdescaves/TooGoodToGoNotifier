namespace TooGoodToGoApi.Models.Responses
{
    public class AuthenticateByPollingIdResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public StartupData StartupData { get; set; }
    }
}