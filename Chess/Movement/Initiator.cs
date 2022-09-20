using Chess.Board;
using Chess.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Chess.Movement
{
    internal sealed class Initiator : IDisposable
    {
        private Vector2 _originalPosition;
        private Vector2 _destination;
        private Vector2 _direction;
        private readonly float _distance;
        private readonly DrawableObject _target;
        public EventHandler DestinationReached;
        public bool IsDisposed { get; private set; }

        public Initiator(Vector2 destination, DrawableObject target, EventHandler destinationReached)
        {
            _originalPosition = target.Position;
            _destination = destination;
            _direction = _destination - target.Position;
            _distance = _direction.Length();
            _direction.Normalize();
            _target = target;
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
            _originalPosition = MovementManager.RecalculateVector(_originalPosition);
            _destination = MovementManager.RecalculateVector(_destination);
            _direction = MovementManager.RecalculateVector(_direction);
        }
        public void Update(GameTime gameTime)
        {
            if (!IsDisposed)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _target.MoveObject(_direction * delta * MovementManager.MovementVelocity);
                Vector2 currentPosition = _target.Position;
                if ((currentPosition - _originalPosition).Length() >= _distance)
                {
                    Vector2 offset = _destination - currentPosition;
                    _target.MoveObject(offset);
                    OnDestinationReached(EventArgs.Empty);
                }
            }
        }

    }
}
