using Chess.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Chess.Graphics.UI;

internal sealed class MainMenu : IDrawableProvider, ITextProvider
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ContentManager _contentManager;
    private readonly SpriteFont _font;
    private readonly Texture2D _buttonTexture;
    private readonly Texture2D _lightTile;
    private readonly Action<ChessGameSettings> _onExit;
    private ChessGameSettings _chessGameSettings;
    private readonly List<DrawableObject> _drawables;
    private readonly List<TextObject> _texts;
    private readonly List<Button> _buttons;
    private const int _pixelBuffer = 10;
    private int _itemHeight;
    private string[] _textValues;
    private string _isWhiteHumanText;
    private string _isBlackHumanText;
    private string _whiteClockTimeText;
    private string _blackClockTimeText;
    private string _whiteIncrementText;
    private string _blackIncrementText;
    private string _FENText;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public MainMenu(GraphicsDevice graphicsDevice, ContentManager contentManager, Action<ChessGameSettings> onExit)
    {
        _graphicsDevice = graphicsDevice;
        _contentManager = contentManager;
        _font = _contentManager.Load<SpriteFont>("menuItems");
        _buttonTexture = _contentManager.Load<Texture2D>("arrow");
        _lightTile = _contentManager.Load<Texture2D>("lightTile");
        _chessGameSettings = new ChessGameSettings();
        _drawables = new List<DrawableObject>();
        _buttons = new List<Button>();
        _texts = new List<TextObject>();
        AssignTextValues();
        SetSize();
        CreateTexts();
        CreateMenuItems();
        _onExit = onExit;
    }
    private void OnExit()
    {
        _onExit?.Invoke(_chessGameSettings);
    }
    private void SetSize()
    {      
        Vector2 measurements = _font.MeasureString(_FENText);
        _itemHeight = (int)measurements.Y;
        Height = _pixelBuffer + (int)((_textValues.Length + 1) * (measurements.Y + _pixelBuffer));
        Width = (int)(_pixelBuffer * 3 + measurements.X + measurements.Y / 2);
    }
    private void AssignTextValues()
    {
        _isWhiteHumanText = $"White played by human: {_chessGameSettings.IsWhiteHuman}";
        _isBlackHumanText = $"Black played by human: {_chessGameSettings.IsBlackHuman}";
        _whiteClockTimeText = $"White's time: {_chessGameSettings.WhiteClockTime.TotalMinutes,3} [min]";
        _blackClockTimeText = $"Black's time: {_chessGameSettings.BlackClockTime.TotalMinutes,3} [min]";
        _whiteIncrementText = $"White's increment: {_chessGameSettings.WhiteIncrement.TotalSeconds,3} [sec]";
        _blackIncrementText = $"Black's increment: {_chessGameSettings.BlackIncrement.TotalSeconds,3} [sec]";
        _FENText = $"Starting position: {_chessGameSettings.FEN}";

        _textValues = new[] { _isWhiteHumanText, _isBlackHumanText, _whiteClockTimeText, _blackClockTimeText, _whiteIncrementText, _blackIncrementText, _FENText };
}
    private void CreateTexts()
    {
        for (int i = 0; i < _textValues.Length; ++i)
        {
            _texts.Add(new TextObject(_font, _textValues[i], new Vector2(_pixelBuffer, _itemHeight * i + _pixelBuffer), Color.Black, 1f));
        }
    }
    private int GetTextWidth(string text) => (int)_font.MeasureString(text).X;
    private void CreateMenuItems()
    {
        ButtonActionInfo actions = new();
        int height = _itemHeight;
        int width = _itemHeight / 2;
        int buttonBuffer = _pixelBuffer * 4;
        actions.OnRelease = () => _chessGameSettings.IsWhiteHuman = !_chessGameSettings.IsWhiteHuman;
        Rectangle dest = new Rectangle((int)_texts[0].Position.X + GetTextWidth(_texts[0].Text) + buttonBuffer, (int)_texts[0].Position.Y, width, height);
        _buttons.Add(CreateArrowButton(dest, true, actions));
        dest = new Rectangle((int)_texts[1].Position.X + GetTextWidth(_texts[1].Text) + buttonBuffer, (int)_texts[1].Position.Y, width, height);
        actions.OnRelease = () => _chessGameSettings.IsBlackHuman = !_chessGameSettings.IsBlackHuman;
        _buttons.Add(CreateArrowButton(dest, true, actions));

        actions.OnRelease = () => _chessGameSettings.WhiteClockTime = TimeSpan.FromMinutes(ChangeValue(_chessGameSettings.WhiteClockTime.TotalMinutes, 1));
        dest = new Rectangle((int)_texts[2].Position.X + GetTextWidth(_texts[2].Text) + buttonBuffer, (int)_texts[2].Position.Y, width, height);
        _buttons.Add(CreateArrowButton(dest, true, actions));
        dest = dest with { X = dest.X + width + buttonBuffer };
        actions.OnRelease = () => _chessGameSettings.WhiteClockTime = TimeSpan.FromMinutes(ChangeValue(_chessGameSettings.WhiteClockTime.TotalMinutes, -1));
        _buttons.Add(CreateArrowButton(dest, false, actions));

        actions.OnRelease = () => _chessGameSettings.BlackClockTime = TimeSpan.FromMinutes(ChangeValue(_chessGameSettings.BlackClockTime.TotalMinutes, 1));
        dest = new Rectangle((int)_texts[3].Position.X + GetTextWidth(_texts[3].Text) + buttonBuffer, (int)_texts[3].Position.Y, width, height);
        _buttons.Add(CreateArrowButton(dest, true, actions));
        dest = dest with { X = dest.X + width + buttonBuffer };
        actions.OnRelease = () => _chessGameSettings.BlackClockTime = TimeSpan.FromMinutes(ChangeValue(_chessGameSettings.BlackClockTime.TotalMinutes, -1));
        _buttons.Add(CreateArrowButton(dest, false, actions));

        actions.OnRelease = () => _chessGameSettings.WhiteIncrement = TimeSpan.FromSeconds(ChangeValue(_chessGameSettings.WhiteIncrement.TotalSeconds, 1, true));
        dest = new Rectangle((int)_texts[4].Position.X + GetTextWidth(_texts[4].Text) + buttonBuffer, (int)_texts[4].Position.Y, width, height);
        _buttons.Add(CreateArrowButton(dest, true, actions));
        dest = dest with { X = dest.X + width + buttonBuffer };
        actions.OnRelease = () => _chessGameSettings.WhiteIncrement = TimeSpan.FromSeconds(ChangeValue(_chessGameSettings.WhiteIncrement.TotalSeconds, -1, true));
        _buttons.Add(CreateArrowButton(dest, false, actions));

        actions.OnRelease = () => _chessGameSettings.BlackIncrement = TimeSpan.FromSeconds(ChangeValue(_chessGameSettings.BlackIncrement.TotalSeconds, 1, true));
        dest = new Rectangle((int)_texts[5].Position.X + GetTextWidth(_texts[5].Text) + buttonBuffer, (int)_texts[5].Position.Y, width, height);
        _buttons.Add(CreateArrowButton(dest, true, actions));
        dest = dest with { X = dest.X + width + buttonBuffer };
        actions.OnRelease = () => _chessGameSettings.BlackIncrement = TimeSpan.FromSeconds(ChangeValue(_chessGameSettings.BlackIncrement.TotalSeconds, -1, true));
        _buttons.Add(CreateArrowButton(dest, false, actions));

        actions.OnRelease = () => OnExit();
        Button startGameButton = CreateStartGameButton(actions);
        _buttons.Add(startGameButton);
    }
    private static double ChangeValue(double value, double amount, bool canBeZero = false) => Math.Clamp(value + amount, canBeZero ? 0 : 1, 999);
    private Button CreateArrowButton(Rectangle destinationRectangle, bool willIncrement, ButtonActionInfo actions)
    {   
        int posX = willIncrement ? 0 : _buttonTexture.Width / 2;
        Model model = new(_buttonTexture, posX, 0, _buttonTexture.Width / 2, _buttonTexture.Height);
        DrawableObject arrow = new DrawableObject(model, destinationRectangle with { X = 0, Y = 0});
        return new Button(_graphicsDevice, _lightTile, new[] { arrow }, null, destinationRectangle, actions);
    }
    private Button CreateStartGameButton(ButtonActionInfo actions)
    {
        Texture2D lightTile = _contentManager.Load<Texture2D>("lightTile");
        string text = "Start game";
        int width = (int)_font.MeasureString(text).X;
        Rectangle destinationRectangle = new Rectangle(Width/2 - width / 2, Height - _itemHeight - _pixelBuffer, width, _itemHeight);
        TextObject textObject = new TextObject(_font, text, Vector2.Zero, Color.White, 1f);
        return new Button(_graphicsDevice, lightTile, null, new[] { textObject }, destinationRectangle, actions);
    }
    public IEnumerable<DrawableObject> GetDrawableObjects() => _drawables.ToArray();
    public IEnumerable<TextObject> GetTextObjects()
    {
        AssignTextValues();
        for(int i = 0; i < _textValues.Length; ++i)
        {
            _texts[i].Text = _textValues[i];
        }
        return _texts.ToArray();
    }
    public Button[] GetButtons() => _buttons.ToArray();
}
