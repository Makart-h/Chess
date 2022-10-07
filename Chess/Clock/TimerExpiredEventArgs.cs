using Chess.AI;
using System;

namespace Chess.Clock;

internal sealed class TimerExpiredEventArgs : EventArgs
{
    public Controller VictoriousController { get; init; }

    public TimerExpiredEventArgs(Controller victoriousController)
    {
        VictoriousController = victoriousController;
    }
}
