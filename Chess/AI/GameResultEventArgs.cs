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
