namespace Chess.AI
{
    internal sealed class GameResultEventArgs
    {
        public GameResult GameResult;

        public GameResultEventArgs(GameResult gameResult)
        {
            GameResult = gameResult;
        }
    }
}
