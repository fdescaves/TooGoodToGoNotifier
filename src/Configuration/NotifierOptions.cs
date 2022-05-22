using System;

namespace TooGoodToGoNotifier.Configuration
{
    public class NotifierOptions
    {
        public int ScanningInterval { get; set; }

        public string RefreshAccessTokenCronExpression { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string[] DefaultRecipients { get; set; } = Array.Empty<string>();

        public FilteredBaskets[] SubscribedBasketsIdByRecipients { get; set; } = Array.Empty<FilteredBaskets>();
    }
}
