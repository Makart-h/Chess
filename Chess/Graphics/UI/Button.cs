using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Chess.Graphics.UI;

internal sealed class Button : IDisposable
{
    public RenderTarget2D Texture { get; init; }
    public Rectangle DestinationRectangle { get; init; }
    public Color Color { get; private set; }
    private bool _isDisposed;
    private readonly bool _isRepeatable;
    private Action _onClick;
    private Action _onRelease;
    private Action _onHoverStarted;
    private Action _onHoverEnded;
    private MouseState _previousMouseState;
    private bool _isInteracted;
    private bool _isHoverd;

    public Button(GraphicsDevice graphicsDevice, Texture2D background, DrawableObject[] drawablesToDraw, TextObject[] textsToDraw, Rectangle destinationRectangle, ButtonActionInfo actions, bool isRepeatable = true, bool shouldApplyDefaultBehaviour = true)
    {
        SetActions(actions, shouldApplyDefaultBehaviour);
        Texture = new RenderTarget2D(graphicsDevice, destinationRectangle.Width, destinationRectangle.Height);
        DestinationRectangle = destinationRectangle;
        Color = Color.White;
        PrepareTexture(background, drawablesToDraw, textsToDraw);
        _isRepeatable = isRepeatable;
    }
    private void SetActions(ButtonActionInfo actions, bool shouldApplyDefaultBehaviour) 
    {
        if (shouldApplyDefaultBehaviour)
        {
            _onClick = new Action(SetClickColor);
            _onRelease = new Action(ResetColor);
        }

        if(actions.OnRelease != null)
            _onRelease += actions.OnRelease;
        
        if(actions.OnClick != null)
            _onClick += actions.OnClick;

        if(actions.OnHoverStarted != null)
            _onHoverStarted = new Action(actions.OnHoverStarted);

        if(actions.OnHoverEnded != null)
            _onHoverEnded = new Action(actions.OnHoverEnded);
    }
    private void PrepareTexture(Texture2D background, DrawableObject[] drawables, TextObject[] texts) 
    {
        var spriteBatch = new SpriteBatch(Texture.GraphicsDevice);
        Texture.GraphicsDevice.SetRenderTarget(Texture);
        spriteBatch.Begin();
        if(background != null)
            spriteBatch.Draw(background, new Rectangle(0,0, Texture.Width, Texture.Height), Color);
        if (drawables != null)
        {
            foreach (DrawableObject drawable in drawables)
            {
                spriteBatch.Draw(drawable.Model.RawTexture, drawable.DestinationRectangle, drawable.Model.TextureRect, Color);
            }
        }
        if (texts != null)
        {
            foreach (TextObject text in texts)
            {
                spriteBatch.DrawString(text.Font, text.Text, text.Position, text.Color);
            }
        }
        spriteBatch.End();
        Texture.GraphicsDevice.SetRenderTarget(null);
    }
    public void Interact(Point point)
    {
        if (!_isDisposed)
        {
            if (DestinationRectangle.Contains(point))
            {
                HandleInteraction();
            }
            else
            {
                StopInteraction();
            }
        }
    }
    private void StopInteraction()
    {
        ResetColor();
        _isInteracted = false;
        if (_isHoverd)
            _onHoverEnded?.Invoke();
    }
    private void HandleInteraction()
    {
        MouseState _currentMouseState = Mouse.GetState();
        if (_isInteracted)
        {       
            // Click.
            if (_previousMouseState.LeftButton == ButtonState.Released && _currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if(_isHoverd)
                    _onHoverEnded?.Invoke();
                _onClick?.Invoke();
            }
            // Release.
            else if (_previousMouseState.LeftButton == ButtonState.Pressed && _currentMouseState.LeftButton == ButtonState.Released)
            {
                _onRelease?.Invoke();
                if (!_isRepeatable)
                    Dispose();
            }
        }
        else
        {
            _isInteracted = true;
            // Click.
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                _onClick?.Invoke();
            }
            // Hover.
            else
            {
                _isHoverd = true;
                _onHoverStarted?.Invoke();
            }
        }
        _previousMouseState = _currentMouseState;
    }
    private void ResetColor() => Color = Color.White;
    private void SetClickColor() => Color = Color.Yellow;
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _onRelease = null;
            _onClick = null;
            _onHoverEnded = null;
            _onHoverEnded = null;
        }
    }
}
