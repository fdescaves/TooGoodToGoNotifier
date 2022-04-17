using System;

namespace TooGoodToGoNotifier.Configuration
{
    public class TooGoodToGoNotifierOptions
    {
        public int Interval { get; set; }

        public string RefreshAccessTokenCronExpression { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string[] Recipients { get; set; }
    }
}
