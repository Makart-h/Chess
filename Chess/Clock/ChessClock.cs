using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Chess.AI;
using Chess.Pieces;

namespace Chess.Clock
{
    internal class ChessClock
    {
        private readonly Dictionary<Team, Controller> controllers;
        private readonly Dictionary<Team, ClockTimer> timers;
        private readonly Dictionary<Team, Double> increment;
        private Team activeTeam;
        public Controller ActiveController { get => controllers[activeTeam]; }
        public static event EventHandler<TimerExpiredEventArgs> TimerExpired;

        public ChessClock(Controller white, Controller black, TimeSpan interval, TimeSpan increment, Team activeTeam)
        {
            controllers = new Dictionary<Team, Controller>();
            timers = new Dictionary<Team, ClockTimer>();
            this.increment = new Dictionary<Team, double>();
            this.activeTeam = activeTeam;
            this.increment[Team.White] = increment.TotalMilliseconds;
            this.increment[Team.Black] = increment.TotalMilliseconds;
            controllers[Team.White] = white;
            controllers[Team.Black] = black;
            timers[Team.White] = new ClockTimer(interval.TotalMilliseconds);
            timers[Team.White].Elapsed += OnTimerExpired;
            timers[Team.Black] = new ClockTimer(interval.TotalMilliseconds);
            timers[Team.Black].Elapsed += OnTimerExpired;
            Controller.MoveChosen += OnMoveChosen;
            Controller.NoMovesAvailable += OnNoMovesAvailable;
        }

        public void OnMoveChosen(object sender, MoveChosenEventArgs args) => Toggle();

        public void OnTimerExpired(object sender, ElapsedEventArgs args)
        {
            activeTeam = activeTeam == Team.White ? Team.Black : Team.White;
            timers[activeTeam].Stop();
            TimerExpired?.Invoke(this, new TimerExpiredEventArgs(controllers[activeTeam]));
        }
        private void OnNoMovesAvailable(object sender, NoMovesEventArgs args)
        {
            timers[args.Team].Pause();
            activeTeam = args.Team == Team.White ? Team.Black : Team.White;
            timers[activeTeam].Pause();
        }
        public void Start()
        {
            timers[activeTeam].Start();
        }
        private void Toggle()
        {
            timers[activeTeam].Pause();
            timers[activeTeam].Increment(increment[activeTeam]);
            activeTeam = activeTeam == Team.White ? Team.Black : Team.White;
            timers[activeTeam].Resume();
        }

        public (TimeSpan WhiteTimer, TimeSpan BlackTimer) GetTimers() => (TimeSpan.FromMilliseconds(timers[Team.White].RemainingTime), TimeSpan.FromMilliseconds(timers[Team.Black].RemainingTime));
    }
}