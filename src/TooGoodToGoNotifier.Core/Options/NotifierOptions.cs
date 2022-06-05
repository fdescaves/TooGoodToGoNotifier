using System;

namespace TooGoodToGoNotifier.Core.Options
{
    public class NotifierOptions
    {
        public int ScanningInterval { get; set; }

        public string RefreshAccessTokenCronExpression { get; set; }

        public string SynchronizeFavoriteBasketsCronExpression { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
