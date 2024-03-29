﻿using Chess.Board;

namespace Chess.Movement;

internal readonly struct Move
{
    public readonly Square Former { get; }
    public readonly Square Latter { get; }
    public readonly MoveType Description { get; }

    public Move(in Square former, in Square latter, MoveType description)
    {
        Former = former;
        Latter = latter;
        Description = description;
    }
}
