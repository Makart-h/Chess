using Microsoft.Xna.Framework;
using System;

namespace Chess.Input;

internal sealed class KeyInput : IDisposable
{
    private bool _isPressed;
    private double _timeSinceClick;
    private readonly double _cooldownInMilliseconds;
    private Action _onPressed;
    private bool _isDisposed;

    public KeyInput(Action onPressed, double cooldownInMilliseconds)
    {
        _onPressed = new Action(onPressed);
        _cooldownInMilliseconds = cooldownInMilliseconds;
    }
    public void AddAction(Action action)
    {
        if (action != null)
            _onPressed += action;
    }
    public bool TryPress(GameTime gameTime)
    {
        if (!_isPressed || _timeSinceClick >= _cooldownInMilliseconds)
        {
            _isPressed = true;
            _timeSinceClick = 0;
            _onPressed?.Invoke();
            return true;
        }
        else
        {
            _timeSinceClick += gameTime.ElapsedGameTime.TotalMilliseconds;
            return false;
        }
    }
    public void Release()
    {
        _isPressed = false;
        _timeSinceClick = 0;
    }
    public void Dispose()
    {
        if(_isDisposed)
            return;

        _isDisposed = true;
        _onPressed = null;
    }
}
