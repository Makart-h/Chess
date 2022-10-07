using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Chess.Input;

internal sealed class InputManager : IDisposable
{

    private readonly Dictionary<Keys, KeyInput> _inputEvents;
    private readonly Stack<Keys> _keysToCheckForRealease;
    private const double _defaultCooldownInMilliseconds = 500d;
    private bool _isDisposed;
    public InputManager()
    {
        _inputEvents = new();
        _keysToCheckForRealease = new();
    }
    public void SubscribeToEvent(Keys key, Action method, double cooldownInMilliseconds = _defaultCooldownInMilliseconds)
    {
        ThrowIfDisposed();
        if (_inputEvents.TryGetValue(key, out var value))
            value.AddAction(method);
        else
            _inputEvents[key] = new KeyInput(method, cooldownInMilliseconds);
    }
    public void CheckInput(GameTime gameTime)
    {
        ThrowIfDisposed();
        CheckReleasedKeys();
        CheckPressedKeys(gameTime);
    }
    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(InputManager));
    }
    private void CheckReleasedKeys()
    {
        foreach (Keys key in _keysToCheckForRealease)
        {
            if (Keyboard.GetState().IsKeyUp(key))
                _inputEvents[key].Release();
        }
    }
    private void CheckPressedKeys(GameTime gameTime)
    {
        foreach (Keys key in Keyboard.GetState().GetPressedKeys())
        {
            if (_inputEvents.TryGetValue(key, out var value))
            {
                if (value.TryPress(gameTime))
                    _keysToCheckForRealease.Push(key);
            }
        }
    }
    public void Dispose()
    {
        if (_isDisposed)
            return;

        foreach(Keys key in _inputEvents.Keys)
        {
            _inputEvents[key].Dispose();
        }
        _inputEvents.Clear();
        _isDisposed = true;
    }
}
