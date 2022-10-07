using Chess.Board;

namespace Chess.Graphics;

    internal sealed class SquareOverlay : DrawableObject
    {
        public SquareOverlayType Type { get; private set; }
        public SquareOverlay(SquareOverlayType type, Square square) : base(null, Chessboard.Instance.ToCordsFromSquare(square))
        {
            Model = new Model(Game1.Instance.Overlays, Square.SquareWidth * (int)type, 0, Square.SquareWidth, Square.SquareHeight);
            Type = type;
        }
    }
