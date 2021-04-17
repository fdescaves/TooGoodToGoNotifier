using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class EmailNotifier : IEmailNotifier
    {
        private readonly ILogger<EmailNotifier> _logger;
        private readonly EmailNotifierOptions _emailNotifierOptions;

        public EmailNotifier(ILogger<EmailNotifier> logger, IOptions<EmailNotifierOptions> emailNotifierOptions)
        {
            _logger = logger;
            _emailNotifierOptions = emailNotifierOptions.Value;
        }

        public void Notify(List<Basket> baskets)
        {
            if (baskets == null || baskets.Count == 0)
            {
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(string.Empty, _emailNotifierOptions.SmtpUserName));

            foreach (var recipient in _emailNotifierOptions.Recipients)
            {
                message.To.Add(new MailboxAddress(string.Empty, recipient));
            }

            message.Subject = "New available basket(s)";

            var stringBuilder = new StringBuilder();
            foreach (var basket in baskets)
            {
                stringBuilder.AppendLine($"{basket.ItemsAvailable} basket(s) available at \"{basket.DisplayName}\"");
            }

            message.Body = new TextPart
            {
                Text = stringBuilder.ToString()
            };

            using var client = new SmtpClient();

            client.Connect(_emailNotifierOptions.SmtpServer, _emailNotifierOptions.SmtpPort, _emailNotifierOptions.UseSsl);

            client.Authenticate(_emailNotifierOptions.SmtpUserName, _emailNotifierOptions.SmtpPassword);

            client.Send(message);

            client.Disconnect(true);
        }
    }
}
