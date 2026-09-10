using System.Diagnostics;

namespace Teleprompter.Services
{
    public interface IScrollEngine
    {
        bool IsPlaying { get; }
        double ElapsedSeconds { get; }
        void Play();
        void Pause();
        void Stop();
        void SeekBy(double seconds);
        void SeekTo(double seconds);
    }

    public class ScrollEngine : IScrollEngine
    {
        private Stopwatch? _stopwatch;
        private double _elapsed;

        public bool IsPlaying => _stopwatch?.IsRunning == true;

        public double ElapsedSeconds =>
            _elapsed + (_stopwatch?.Elapsed.TotalSeconds ?? 0);

        public void Play()
        {
            if (IsPlaying)
                return;

            _stopwatch = Stopwatch.StartNew();
        }

        public void Pause()
        {
            if (_stopwatch is { IsRunning: true })
            {
                _elapsed += _stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Stop();
            }
        }

        public void Stop()
        {
            _stopwatch?.Stop();
            _stopwatch = null;
            _elapsed = 0;
        }

        public void SeekBy(double seconds)
        {
            var target = Math.Max(0, ElapsedSeconds + seconds);
            _elapsed = target;
            _stopwatch?.Restart();
        }

        public void SeekTo(double seconds)
        {
            if (_stopwatch is { IsRunning: true })
            {
                _elapsed += _stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Stop();
            }

            _elapsed = Math.Max(0, seconds);
        }
    }
}
