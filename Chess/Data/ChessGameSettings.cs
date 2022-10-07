using System;

namespace Chess.Data;

internal struct ChessGameSettings
{
    public bool IsWhiteHuman { get; set; }
    public bool IsBlackHuman { get; set; }
    public TimeSpan WhiteClockTime { get; set; }
    public TimeSpan BlackClockTime { get; set; }
    public TimeSpan WhiteIncrement { get; set; }
    public TimeSpan BlackIncrement { get; set; }
    public string FEN { get; set; }

    public ChessGameSettings()
    {
        IsWhiteHuman = true;
        IsBlackHuman = false;
        WhiteClockTime = TimeSpan.FromMinutes(10);
        BlackClockTime = TimeSpan.FromMinutes(10);
        WhiteIncrement = TimeSpan.Zero;
        BlackIncrement = TimeSpan.Zero;
        FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    }
}
