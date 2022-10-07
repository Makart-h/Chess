using System;

namespace Chess.Pieces.Info;

[Flags]
public enum CastlingRights
{
    None = 0,
    KingSide = 1,
    QueenSide = 2
}
