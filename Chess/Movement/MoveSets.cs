using System;

namespace Chess.Movement;

[Flags]
enum MoveSets
{
    Pawn = 1,
    Horizontal = 1 << 1,
    Vertical = 1 << 2,
    Diagonal = 1 << 3,
    Knight = 1 << 4,
    Rook = Vertical | Horizontal,
    Bishop = Diagonal,
    King = 1 << 5,
    Queen = Rook | Bishop
}