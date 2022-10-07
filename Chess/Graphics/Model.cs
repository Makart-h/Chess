using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics;

internal sealed class Model
{
    public Texture2D RawTexture { get; private set; }
    public Rectangle TextureRect { get; private set; }

    public Model(Texture2D rawTexture, int posX, int posY, int width, int height)
    {
        RawTexture = rawTexture;
        TextureRect = new Rectangle(posX, posY, width, height);
    }
    public Model(Model other)
    {
        RawTexture = other.RawTexture;
        TextureRect = new Rectangle(other.TextureRect.X, other.TextureRect.Y, other.TextureRect.Width, other.TextureRect.Height);
    }
    public void Offset(int x, int y)
    {
        TextureRect = new Rectangle(TextureRect.X + x, TextureRect.Y + y, TextureRect.Width, TextureRect.Height);
    }
}
