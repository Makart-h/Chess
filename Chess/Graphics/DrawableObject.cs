using Chess.Movement;
using Microsoft.Xna.Framework;

namespace Chess.Graphics
{
    abstract class DrawableObject
    {
        public virtual Model Model { get; protected set; }
        public virtual Vector2 Position { get; protected set; }
        protected Color _color = Color.White;
        public virtual Color Color { get => _color; }

        public void MoveObject(Vector2 vector) => Position += vector;
        public void RecalculatePosition() => Position = MovementManager.RecalculateVector(Position);
    }
}
