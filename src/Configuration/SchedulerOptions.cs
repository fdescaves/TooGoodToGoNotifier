using System;

namespace TooGoodToGoNotifier.Configuration
{
    public class SchedulerOptions
    {
        public int Interval { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
