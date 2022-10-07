using Chess.Pieces;
using System;

namespace Chess.Positions.Evaluators;

internal struct ExchangeParticipant : IComparable, IComparable<ExchangeParticipant>
{
    public (int X, int Y) Direction { get; init; }
    public Piece Piece { get; init; }

    public ExchangeParticipant((int, int) direction, Piece piece)
    {
        Direction = direction;
        Piece = piece;
    }
    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        else if (obj is ExchangeParticipant e)
            return CompareTo(e);
        else
            throw new ArgumentException("Object is not an Evaluation!");
    }
    public int CompareTo(ExchangeParticipant other) => Piece.CompareTo(other.Piece);
}
