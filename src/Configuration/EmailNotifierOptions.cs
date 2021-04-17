namespace TooGoodToGoNotifier.Configuration
{
    public class EmailNotifierOptions
    {
        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public bool UseSsl { get; set; }

        public string SmtpUserName { get; set; }

        public string SmtpPassword { get; set; }

        public string[] Recipients { get; set; }
    }
}
