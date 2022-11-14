using Microsoft.Xna.Framework;

namespace Chess.Movement;

internal interface IMovable
{
    public Vector2 Position { get; set; }
}
