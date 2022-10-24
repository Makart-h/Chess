using System;

namespace Chess.Positions.Evaluators;

[Flags]
internal enum PawnDetails
{
    None = 0,
    Connected = 1,
    Passed = 1 << 1,
    Backward = 1 << 2,
    Isolated = 1 << 3,
    HeadsTowardsHigherRanks = 1 << 4
}