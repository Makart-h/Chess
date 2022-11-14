using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics;

internal sealed class DrawableObject : IDrawable
{
    public Texture2D RawTexture { get; set; }
    public Rectangle TextureRect { get; set; }
    public Rectangle DestinationRect { get; set; }
    public Color Color { get; set; }

    public DrawableObject(Texture2D rawTexture, Rectangle textureRect, Rectangle destinationRect)
    {
        RawTexture = rawTexture;
        TextureRect = textureRect;
        DestinationRect = destinationRect;
        Color = Color.White;
    }
}
