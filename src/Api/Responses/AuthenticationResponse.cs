namespace TooGoodToGoNotifier.Api.Responses
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public StartupData StartupData { get; set; }
    }
}
