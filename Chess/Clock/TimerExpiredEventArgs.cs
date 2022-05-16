using System;
using System.Collections.Generic;
using System.Text;
using Chess.AI;

namespace Chess.Clock
{
    internal class TimerExpiredEventArgs : EventArgs
    {
        public readonly Controller controller;

        public TimerExpiredEventArgs(Controller controller)
        {
            this.controller = controller;
        }
    }
}
