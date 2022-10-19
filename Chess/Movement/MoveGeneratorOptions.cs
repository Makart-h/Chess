using System;

namespace Chess.Movement;

[Flags]
internal enum MoveGeneratorOptions
{
    None = 0,
    InfiniteRange = 1,
    MovesIncluded = 1 << 1,
    TakesIncluded = 1 << 2,
    DefendsIncluded = 1 << 3
}