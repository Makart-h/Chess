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
        private readonly Dictionary<Team, Timer> timers;
        private readonly Dictionary<Team, Double> increment;
        private Team activeTeam;
        public Controller ActiveController { get => controllers[activeTeam]; }
        public static event EventHandler<TimerExpiredEventArgs> TimerExpired;

        public ChessClock(Controller white, Controller black, TimeSpan interval, TimeSpan increment, Team activeTeam)
        {
            controllers = new Dictionary<Team, Controller>();
            timers = new Dictionary<Team, Timer>();
            this.increment = new Dictionary<Team, double>();
            this.activeTeam = activeTeam;
            this.increment[Team.White] = increment.TotalMilliseconds;
            this.increment[Team.Black] = increment.TotalMilliseconds;
            controllers[Team.White] = white;
            controllers[Team.Black] = black;
            timers[Team.White] = new Timer(interval.TotalMilliseconds);
            timers[Team.White].Elapsed += OnTimerExpired;
            timers[Team.Black] = new Timer(interval.TotalMilliseconds);
            timers[Team.Black].Elapsed += OnTimerExpired;
            Piece.PieceMoved += OnPieceMoved;
        }

        public void OnPieceMoved(object sender, PieceMovedEventArgs args) => Toggle();

        public void OnTimerExpired(object sender, ElapsedEventArgs args)
        {
            TimerExpired?.Invoke(this, new TimerExpiredEventArgs(controllers[activeTeam]));
            activeTeam = activeTeam == Team.White ? Team.Black : Team.White;
            timers[activeTeam].Stop();         
        }
        public void Start()
        {
            timers[activeTeam].Start();
        }
        private void Toggle()
        {
            timers[activeTeam].Interval += increment[activeTeam];
            timers[activeTeam].Stop();
            activeTeam = activeTeam == Team.White ? Team.Black : Team.White;
            timers[activeTeam].Start();
        }
    }
}
