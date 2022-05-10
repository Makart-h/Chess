using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Graphics
{
    class Model
    {
        private readonly Texture2D rawTexture;
        public Texture2D RawTexture { get => rawTexture; }
        private readonly Rectangle texture;
        public Rectangle TextureRect { get => texture; }
        private readonly int width, height;

        public Model(Texture2D rawTexture, int posX, int posY, int width, int height)
        {
            this.rawTexture = rawTexture;
            this.width = width;
            this.height = height;
            this.texture = new Rectangle(posX, posY, width, height);
        }

        public Model(Model other)
        {
            this.rawTexture = other.rawTexture;
            this.width = other.width;
            this.height = other.height;
            this.texture = new Rectangle(other.TextureRect.X, other.TextureRect.Y, other.TextureRect.Width, other.TextureRect.Height);
        }
    }
}
