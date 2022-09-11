using System;
using Chess.AI;

namespace Chess.Clock
{
    internal class TimerExpiredEventArgs : EventArgs
    {
        public readonly Controller VictoriousController;

        public TimerExpiredEventArgs(Controller victoriousController)
        {
            VictoriousController = victoriousController;
        }
    }
}
