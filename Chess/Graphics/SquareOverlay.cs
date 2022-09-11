using Chess.Board;

namespace Chess.Graphics
{
    class SquareOverlay : DrawableObject
    {
        public SquareOverlayType Type { get; private set; }
        public SquareOverlay(SquareOverlayType type, Square square)
        {
            Type = type;
            Model = new Model(Game1.Instance.Overlays, Square.SquareWidth * (int)type, 0, Square.SquareWidth, Square.SquareHeight);
            Position = Chessboard.Instance.ToCordsFromSquare(square);
        }
    }
}
