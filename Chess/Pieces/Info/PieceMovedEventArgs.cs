using Chess.Movement;
using System;

namespace Chess.Pieces.Info;

internal sealed class PieceMovedEventArgs : EventArgs
{
    public Piece Piece { get; init; }
    public Move Move { get; init; }
    public PieceMovedEventArgs(Piece piece, Move move)
    {
        Piece = piece;
        Move = move;
    }
}
