namespace TooGoodToGo.Api.Models.Requests
{
    public class AuthenticateByPollingIdRequest : AuthenticateByEmailRequest
    {
        public string RequestPollingId { get; set; }
    }
}
