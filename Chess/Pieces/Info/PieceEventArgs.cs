using System;

namespace Chess.Pieces.Info;

internal sealed class PieceEventArgs : EventArgs
{
    public Piece Piece { get; init; }

    public PieceEventArgs(Piece piece)
    {
        Piece = piece;
    }
}
