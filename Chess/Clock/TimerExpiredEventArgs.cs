using System;
using System.Collections.Generic;
using System.Text;
using Chess.AI;
using Chess.Pieces;

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
