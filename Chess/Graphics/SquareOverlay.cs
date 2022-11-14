using Chess.Board;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics;

internal sealed class SquareOverlay : IMovableDrawable
{
    private static readonly int s_numberOfTypes = 6;
    private Vector2 _position;
    private Rectangle _destinationRect;
    public SquareOverlayType Type { get; private set; }
    public Texture2D RawTexture { get; set; }
    public Rectangle TextureRect { get; set; }
    public Rectangle DestinationRect { get => _destinationRect; 
        set 
        {
            _destinationRect = value;
            _position.X = value.X;
            _position.Y = value.Y;
        } 
    }
    public Color Color { get; set; }
    public Vector2 Position { get => _position;
        set 
        {
            _position = value;
            _destinationRect.X = (int)_position.X;
            _destinationRect.Y = (int)_position.Y;
        } 
    }
    public SquareOverlay(Texture2D texture, SquareOverlayType type, Square square)
    {
        int squareSideLength = texture.Width / s_numberOfTypes;
        RawTexture = texture;
        TextureRect = new Rectangle(squareSideLength * (int)type, 0, squareSideLength, squareSideLength);
        Vector2 position = Chessboard.Instance.ToCordsFromSquare(square);
        _position = position;
        _destinationRect = new Rectangle((int)position.X, (int)position.Y, Chessboard.Instance.SquareSideLength, Chessboard.Instance.SquareSideLength);
        Color = Color.White;
        Type = type;
    }
}
