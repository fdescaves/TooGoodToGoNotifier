namespace TooGoodToGoNotifier.Api.Requests
{
    public class AuthenticationRequest
    {
        public string DeviceType { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
