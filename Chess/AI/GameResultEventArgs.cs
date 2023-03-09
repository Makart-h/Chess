using System;

namespace Chess.AI;

internal sealed class GameResultEventArgs : EventArgs
{
    public GameResult GameResult;

    public GameResultEventArgs(GameResult gameResult)
    {
        GameResult = gameResult;
    }
}
