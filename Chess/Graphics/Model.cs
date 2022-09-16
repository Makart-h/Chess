using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics
{
    internal sealed class Model
    {
        public Texture2D RawTexture { get; private set; }
        public Rectangle TextureRect { get; private set; }
        private readonly int width, height;

        public Model(Texture2D rawTexture, int posX, int posY, int width, int height)
        {
            RawTexture = rawTexture;
            this.width = width;
            this.height = height;
            TextureRect = new Rectangle(posX, posY, width, height);
        }
        public Model(Model other)
        {
            RawTexture = other.RawTexture;
            width = other.width;
            height = other.height;
            TextureRect = new Rectangle(other.TextureRect.X, other.TextureRect.Y, other.TextureRect.Width, other.TextureRect.Height);
        }
    }
}
