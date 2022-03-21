namespace TooGoodToGoNotifier
{
    public interface IEmailService
    {
        public void SendEmail(string subject, string body, string[] recipients);
    }
}
