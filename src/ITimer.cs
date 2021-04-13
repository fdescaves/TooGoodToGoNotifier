using System.Timers;

namespace TooGoodToGoNotifier
{
    public interface ITimer
    {
        event ElapsedEventHandler Elapsed;

        double Interval { get; set; }

        bool AutoReset { get; set; }

        void Start();

        void Stop();
    }
}
