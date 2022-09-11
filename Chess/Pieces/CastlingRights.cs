using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    [Flags]
    public enum CastlingRights
    {
        None = 0,
        KingSide = 1,
        QueenSide = 2
    }
}
