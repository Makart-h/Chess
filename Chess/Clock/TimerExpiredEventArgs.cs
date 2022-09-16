using System;
using Chess.AI;

namespace Chess.Clock
{
    internal sealed class TimerExpiredEventArgs : EventArgs
    {
        public readonly Controller VictoriousController;

        public TimerExpiredEventArgs(Controller victoriousController)
        {
            VictoriousController = victoriousController;
        }
    }
}
