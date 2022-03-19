namespace TooGoodToGoNotifier.Api.Requests
{
    public class AuthenticateByPollingIdRequest : AuthenticateByEmailRequest
    {
        public string RequestPollingId { get; set; }
    }
}
