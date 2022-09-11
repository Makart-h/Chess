using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Movement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess.Graphics
{
    abstract class DrawableObject
    {
        protected Model model;
        public virtual Model Model { get => model; }
        protected Vector2 position;
        public virtual Vector2 Position { get => position; }
        protected Color color = Color.White;
        public virtual Color Color { get => color; }

        public void MoveObject(Vector2 vector) => position += vector;
        public void RecalculatePosition() => position = MovementManager.RecalculateVector(position);
    }
}
