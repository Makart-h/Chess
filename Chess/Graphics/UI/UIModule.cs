using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Chess.Graphics.UI;

internal sealed class UIModule : IComparable<UIModule>
{
    public GraphicsDevice GraphicsDevice { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    public RenderTarget2D RenderTarget { get; private set; }
    private Vector2 _positionFraction;
    public Vector2 Position { get => new(GraphicsDevice.PresentationParameters.BackBufferWidth * _positionFraction.X, GraphicsDevice.PresentationParameters.BackBufferHeight * _positionFraction.Y); }
    private Vector2 _screenFraction;
    public int Width { get => (int)(GraphicsDevice.PresentationParameters.BackBufferWidth * _screenFraction.X); }
    public int Height { get => (int)(GraphicsDevice.PresentationParameters.BackBufferHeight * _screenFraction.Y); }
    public DrawableObject Background { get; private set; }
    private readonly List<IDrawableProvider> _drawableProviders;
    private readonly List<ITextProvider> _textProviders;
    private readonly List<DrawableObject> _drawables;
    private readonly List<TextObject> _texts;
    private readonly List<Button> _buttons;
    public int Layer {get; set;}
    private readonly Stack<object> _objectsToRemove;
    private readonly Stack<object> _objectsToAdd;
    public UIModule(GraphicsDevice graphicsDevice, DrawableObject background, Vector2 positionFraction, Vector2 screenFraction, IDrawableProvider[] drawableProviders, ITextProvider[] textProviders, Button[] buttons = null)
        : this(graphicsDevice, background, (background.Model.TextureRect.Width, background.Model.TextureRect.Height), positionFraction, screenFraction, drawableProviders, textProviders, buttons)
    {    }
    public UIModule(GraphicsDevice graphicsDevice, DrawableObject background, (int width, int height) renderTargetSize, Vector2 positionFraction, Vector2 screenFraction, IDrawableProvider[] drawableProviders, ITextProvider[] textProviders, Button[] buttons = null)
    {
        GraphicsDevice = graphicsDevice;
        SpriteBatch = new SpriteBatch(graphicsDevice);
        RenderTarget = new RenderTarget2D(graphicsDevice, renderTargetSize.width, renderTargetSize.height);
        _positionFraction = positionFraction;
        _screenFraction = screenFraction;
        Background = background;
        _drawableProviders = drawableProviders == null ? new() : new List<IDrawableProvider>(drawableProviders);
        _textProviders = textProviders == null ? new() : new List<ITextProvider>(textProviders);
        _buttons = buttons == null ? new() : new List<Button>(buttons);
        _texts = new();
        _drawables = new();
        Layer = 0;
        _objectsToRemove = new();
        _objectsToAdd = new();
    }
    public void PrepareRenderTarget()
    {
        GraphicsDevice.SetRenderTarget(RenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch.Begin();
        if(Background != null)
            SpriteBatch.Draw(Background.Model.RawTexture, new Rectangle(0,0,RenderTarget.Width,RenderTarget.Height), Background.Model.TextureRect, Background.Color);
        foreach (IDrawableProvider provider in _drawableProviders)
        {
            foreach (DrawableObject drawable in provider.GetDrawableObjects())
            {
                SpriteBatch.Draw(drawable.Model.RawTexture, drawable.DestinationRectangle, drawable.Model.TextureRect, drawable.Color);
            }
        }
        foreach(DrawableObject drawable in _drawables)
        {
            SpriteBatch.Draw(drawable.Model.RawTexture, drawable.DestinationRectangle, drawable.Model.TextureRect, drawable.Color);
        }
        foreach (ITextProvider textProvider in _textProviders)
        {
            foreach (TextObject text in textProvider.GetTextObjects())
            {
                SpriteBatch.DrawString(text.Font, text.Text, text.Position, text.Color, 0f, Vector2.Zero, text.Scale, SpriteEffects.None, 1f);
            }
        }
        foreach(TextObject text in _texts)
        {
            SpriteBatch.DrawString(text.Font, text.Text, text.Position, text.Color, 0f, Vector2.Zero, text.Scale, SpriteEffects.None, 1f);
        }
        foreach(Button button in _buttons)
        {
            SpriteBatch.Draw(button.Texture, button.DestinationRectangle, button.Color);
        }
        SpriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
    }
    public Point ToLocalCoordinates(Point point)
    {
        double yFactor = RenderTarget.Height / (double)Height;
        double xFactor = RenderTarget.Width / (double)Width;
        return new Point((int)((point.X - Position.X) * xFactor), (int)((point.Y - Position.Y) * yFactor));
    }
    public void Interact(Point cursorPosition)
    {
        if (CursorInBounds(cursorPosition))
        {
            cursorPosition = ToLocalCoordinates(cursorPosition);

            foreach (Button button in _buttons)
            {
                button.Interact(cursorPosition);
            }
        }
        while (_objectsToAdd.TryPop(out object itemToAdd))
            AddElement(itemToAdd);
        while (_objectsToRemove.TryPop(out object itemToRemove))
            RemoveElement(itemToRemove);
    }
    private bool CursorInBounds(Point cursorPosition)
    {
        Rectangle moduleBounds = new((int)Position.X, (int)Position.Y, Width, Height);
        return moduleBounds.Contains(cursorPosition);
    }
    private void AddElement(object element)
    {
        if (element is IDrawableProvider dp)
            _drawableProviders.Add(dp);
        else if (element is ITextProvider tp)
            _textProviders.Add(tp);
        else if (element is Button b)
            _buttons.Add(b);
        else if (element is DrawableObject d)
            _drawables.Add(d);
        else if (element is TextObject t)
            _texts.Add(t);
        else
            throw new ArgumentOutOfRangeException();
    }
    public void SubmitElementToRemove(object element) => _objectsToRemove.Push(element);
    public void SubmitElementToAdd(object element) => _objectsToAdd.Push(element);
    private void RemoveElement(object element)
    {
        if (element is IDrawableProvider dp)
            _drawableProviders.Remove(dp);
        else if (element is ITextProvider tp)
            _textProviders.Remove(tp);
        else if (element is Button b)
            _buttons.Remove(b);
        else if (element is DrawableObject d)
            _drawables.Remove(d);
        else if (element is TextObject t)
            _texts.Remove(t);
        else
            throw new ArgumentOutOfRangeException();
    }
    public int CompareTo(UIModule other) => Layer.CompareTo(other.Layer);
}
