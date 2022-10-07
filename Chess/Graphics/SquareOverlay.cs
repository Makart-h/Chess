using Chess.Board;
using Microsoft.Xna.Framework;

namespace Chess.Graphics;

internal sealed class SquareOverlay : DrawableObject
{
    public SquareOverlayType Type { get; private set; }
    private static readonly int s_numberOfTypes = 6;
    public SquareOverlay(SquareOverlayType type, Square square) : base(null, new Rectangle((int)Chessboard.Instance.ToCordsFromSquare(square).X, (int)Chessboard.Instance.ToCordsFromSquare(square).Y, Chessboard.Instance.SquareSideLength, Chessboard.Instance.SquareSideLength))
    {
        int squareSideLength = OverlayManager.Instance.OverlaysTexture.Width / s_numberOfTypes;
        Model = new Model(OverlayManager.Instance.OverlaysTexture, squareSideLength * (int)type, 0, squareSideLength, squareSideLength);
        Type = type;
    }
}
