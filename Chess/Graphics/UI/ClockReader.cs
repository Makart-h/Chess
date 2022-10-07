using Chess.Board;
using Chess.Clock;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Chess.Graphics.UI;

internal class ClockReader : ITextProvider
{
    private readonly SpriteFont _font;
    private Vector2 _topClockPosition;
    private Vector2 _bottoomClockPosition;
    private readonly TextObject _whiteClock;
    private readonly TextObject _blackClock;
    private const float _propotionalDistanceFromEdges = 0.05f;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public ClockReader(SpriteFont font, float widthProportion, float heightProportion, Color color, float scale = 1f)
    {
        _font = font;
        SetVectors(widthProportion, heightProportion);
        _whiteClock = new TextObject(_font, string.Empty, _bottoomClockPosition, color, scale);
        _blackClock = new TextObject(_font, string.Empty, _topClockPosition, color, scale);
        Chessboard.BoardInverted += OnBoardInverted;
    }
    private void SetVectors(float widthProportion, float heightProportion)
    {
        widthProportion = Math.Clamp(widthProportion, 0, 1);
        heightProportion = Math.Clamp(heightProportion, 0, 1);
        Vector2 measurements = _font.MeasureString("00:00");
        Width = (int)(measurements.X / widthProportion * (1 + _propotionalDistanceFromEdges * 2));
        Height = (int)(measurements.Y / heightProportion);
        int posX = (int)(Width * _propotionalDistanceFromEdges);
        _topClockPosition = new Vector2(posX, 0);
        _bottoomClockPosition = new Vector2(posX, Height - measurements.Y);
    }
    private void GetClocks()
    {
        (TimeSpan whiteTimer, TimeSpan blackTimer) = ChessClock.GetTimers();
        _whiteClock.Text = $"{whiteTimer.Minutes:d2}:{whiteTimer.Seconds:d2}";
        _blackClock.Text = $"{blackTimer.Minutes:d2}:{blackTimer.Seconds:d2}";
        _whiteClock.Position = _bottoomClockPosition;
        _blackClock.Position = _topClockPosition;
    }
    private void OnBoardInverted(object sender, EventArgs e)
    {
        (_bottoomClockPosition, _topClockPosition) = (_topClockPosition, _bottoomClockPosition);
    }
    public TextObject[] GetTextObjects()
    {
        GetClocks();
        return new[] { _whiteClock, _blackClock};
    }
}
