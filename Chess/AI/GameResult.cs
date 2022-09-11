using System;

namespace Chess.AI
{
    public enum GameResult : byte
    {
        White,
        Black,
        Stalemate,
        Draw,
        InProgress,
        ThreefoldRepetition,
        FivefoldRepetition
    }
}
