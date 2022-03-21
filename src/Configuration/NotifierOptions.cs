using System;

namespace TooGoodToGoNotifier.Configuration
{
    public class NotifierOptions
    {
        public int Interval { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string[] Recipients { get; set; }
    }
}
