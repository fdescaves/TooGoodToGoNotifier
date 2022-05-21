using System.Threading.Tasks;

namespace TooGoodToGoNotifier
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string subject, string body, string[] recipients);
    }
}
