using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Chess.Graphics;

internal class TextObject
{
    public SpriteFont Font { get; init; }
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; }
    public float Scale { get; set; }

    public TextObject(SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        Font = font;
        Text = text;
        Position = position;
        Color = color;
        Scale = scale;
    }
}