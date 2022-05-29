namespace TooGoodToGo.Api.Models.Requests
{
    public class AuthenticateByEmailRequest
    {
        public string DeviceType { get; set; }

        public string Email { get; set; }
    }
}
