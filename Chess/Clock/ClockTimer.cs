using System.Diagnostics;
using System.Timers;

namespace Chess.Clock;

    internal sealed class ClockTimer : Timer
    {
        public double RemainingTime { get => _isPaused ? _remainingTime : Interval - _stopwatch.Elapsed.TotalMilliseconds; }
        private double _remainingTime;
        private readonly Stopwatch _stopwatch;
        private bool _isPaused;
        private bool _isInitialized;

        public ClockTimer(double interval) : base(interval)
        {
            _stopwatch = new Stopwatch();
        }

        public new void Start()
        {
            ResetStopwatch();
            _isInitialized = true;
            base.Start();
        }

        private void ResetStopwatch()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        public void Pause()
        {
            Stop();
            _stopwatch.Stop();
            _isPaused = true;
            double delta = Interval - _stopwatch.Elapsed.TotalMilliseconds;
            _remainingTime = delta < 0 ? 0 : delta;
        }

        public void Increment(double increment)
        {
            _remainingTime += increment;
        }

        public void Resume()
        {
            if (!_isInitialized)
                Start();
            else
            {
                Interval = _remainingTime;
                _remainingTime = 0;
                Start();
                _isPaused = false;
            }
        }
    }
