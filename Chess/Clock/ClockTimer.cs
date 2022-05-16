using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;

namespace Chess.Clock
{
    internal class ClockTimer : Timer
    {
        public double RemainingTime { get => paused ? remainingTime : Interval - stopwatch.Elapsed.TotalMilliseconds; }
        private double remainingTime;
        private readonly Stopwatch stopwatch;
        private bool paused;
        private bool initilized;

        public ClockTimer(double interval) : base(interval)
        {
            stopwatch = new Stopwatch();
        }

        public new void Start()
        {
            ResetStopwatch();
            initilized = true;
            base.Start();
        }

        private void ResetStopwatch()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void Pause()
        {
            Stop();
            stopwatch.Stop();
            paused = true;
            remainingTime = Interval - stopwatch.Elapsed.TotalMilliseconds;
        }

        public void Increment(double increment)
        {
            remainingTime += increment;
        }

        public void Resume()
        {
            if (!initilized)
                Start();
            else
            {
                Interval = remainingTime;
                remainingTime = 0;
                Start();
                paused = false;
            }
        }
    }
}
