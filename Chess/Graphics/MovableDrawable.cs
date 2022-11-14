using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics;

internal class MovableDrawable : IMovableDrawable
{
    private Vector2 _position;
    private Rectangle _destinationRect;
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
    public Vector2 Position { get => _position; set
        {
            _position = value;
            _destinationRect.X = (int)value.X;
            _destinationRect.Y = (int)value.Y;
        }
    }
    public MovableDrawable(Vector2 position, Texture2D rawTexture, Rectangle textureRect, Rectangle destinationRect)
    {
        _position = position;
        RawTexture = rawTexture;
        TextureRect = textureRect;
        _destinationRect = destinationRect;
        Color = Color.White;
    }
}
