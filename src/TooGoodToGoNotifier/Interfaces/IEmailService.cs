using System.Threading.Tasks;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string subject, string body, string[] recipients);
    }
}
