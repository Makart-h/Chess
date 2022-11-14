using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics;

internal interface IDrawable
{
    public Texture2D RawTexture { get; set; }
    public Rectangle TextureRect { get; set; }
    public Rectangle DestinationRect { get; set; }
    public Color Color { get; set; }
}
