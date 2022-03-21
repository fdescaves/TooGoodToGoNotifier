using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailServiceOptions _emailNotifierOptions;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailServiceOptions> emailNotifierOptions)
        {
            _logger = logger;
            _emailNotifierOptions = emailNotifierOptions.Value;
        }

        public void SendEmail(string subject, string body, string[] recipients)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(string.Empty, _emailNotifierOptions.SmtpUserName));

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailboxAddress(string.Empty, recipient));
            }

            message.Subject = subject;

            message.Body = new TextPart
            {
                Text = body
            };

            using var client = new SmtpClient();

            client.Connect(_emailNotifierOptions.SmtpServer, _emailNotifierOptions.SmtpPort, _emailNotifierOptions.UseSsl);

            client.Authenticate(_emailNotifierOptions.SmtpUserName, _emailNotifierOptions.SmtpPassword);

            client.Send(message);

            client.Disconnect(true);
        }
    }
}
