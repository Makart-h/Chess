using Chess.Board;
using System.Collections.Generic;

namespace Chess.Movement;

internal sealed class Move
{
    public Square Former { get; private set; }
    public Square Latter { get; private set; }
    public char Description { get; private set; }

    public Move(Square former, Square latter, char description)
    {
        Former = former;
        Latter = latter;
        Description = description;
    }
    public Move(Move other)
    {
        Former = other.Former;
        Latter = other.Latter;
        Description = other.Description;
    }
}
