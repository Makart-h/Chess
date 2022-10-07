using Chess.AI;
using System;

namespace Chess.Clock;

    internal sealed class TimerExpiredEventArgs : EventArgs
    {
        public readonly Controller VictoriousController;

        public TimerExpiredEventArgs(Controller victoriousController)
        {
            VictoriousController = victoriousController;
        }
    }
