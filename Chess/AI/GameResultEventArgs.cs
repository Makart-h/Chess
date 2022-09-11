using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.AI
{
    internal class GameResultEventArgs
    {
        public GameResult GameResult;

        public GameResultEventArgs(GameResult gameResult)
        {
            GameResult = gameResult;
        }
    }
}
