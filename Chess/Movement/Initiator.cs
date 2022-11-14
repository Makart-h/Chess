using Chess.Board;
using Microsoft.Xna.Framework;
using System;

namespace Chess.Movement;

internal sealed class Initiator : IInitiator
{
    private Vector2 _originalPosition;
    private Vector2 _destination;
    private Vector2 _direction;
    private readonly float _distance;
    private readonly IMovable _target;
    public bool IsDisposed { get; private set; }
    public EventHandler DestinationReached { get; set; }
    public IMovable Target { get => _target; set { } }
    public float Velocity { get; set; }

    public Initiator(Vector2 destination, IMovable target, float velocity, EventHandler onDestinationReached)
    {
        _originalPosition = target.Position;
        _destination = destination;
        _direction = _destination - target.Position;
        _distance = _direction.Length();
        _direction.Normalize();
        _target = target;
        Velocity = velocity;
        DestinationReached += onDestinationReached;
        Chessboard.BoardInverted += OnBoardInverted;
    }
    public void Dispose()
    {
        if (IsDisposed)
            return;

        DestinationReached = null;
        Chessboard.BoardInverted -= OnBoardInverted;
    }
    private void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Initiator));
    }
    private void OnBoardInverted(object sender, EventArgs args)
    {
        _originalPosition = MovementManager.RecalculateVector(_originalPosition);
        _destination = MovementManager.RecalculateVector(_destination);
        _direction = MovementManager.RecalculateVector(_direction);
    }
    public void Update(GameTime gameTime)
    {
        ThrowIfDisposed();
        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _target.Position += _direction * delta * Velocity;

        if ((_target.Position - _originalPosition).Length() >= _distance)
        {
            _target.Position = _destination;
            DestinationReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
