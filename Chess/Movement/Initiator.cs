using Chess.Board;
using Chess.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Movement
{
    internal sealed class Initiator : IDisposable
    {
        private Vector2 originalPosition;
        private Vector2 destination;
        private Vector2 direction;
        private readonly float distance;
        private readonly DrawableObject target;
        public EventHandler DestinationReached;
        public bool IsDisposed { get; private set; }

        public Initiator(Vector2 destination, DrawableObject target, EventHandler destinationReached)
        {
            originalPosition = target.Position;
            this.destination = destination;
            direction = this.destination - target.Position;
            distance = direction.Length();
            direction.Normalize();
            this.target = target;
            DestinationReached += destinationReached;
            Chessboard.BoardInverted += OnBoardInverted;
        }

        public void Dispose()
        {
            DestinationReached = null;
            Chessboard.BoardInverted -= OnBoardInverted;
        }

        private void OnDestinationReached(EventArgs args)
        {
            DestinationReached?.Invoke(this, args);
        }
        private void OnBoardInverted(object sender, EventArgs args)
        {
            originalPosition = MovementManager.RecalculateVector(originalPosition);
            destination = MovementManager.RecalculateVector(destination);
            direction = MovementManager.RecalculateVector(direction);
        }

        public void Update(GameTime gameTime)
        {
            if (!IsDisposed)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                target.MoveObject(direction * delta * MovementManager.MovementVelocity);
                Vector2 currentPosition = target.Position;
                if ((currentPosition - originalPosition).Length() >= distance)
                {
                    Vector2 offset = destination - currentPosition;
                    target.MoveObject(offset);
                    OnDestinationReached(EventArgs.Empty);
                }
            }
        }

    }
}
