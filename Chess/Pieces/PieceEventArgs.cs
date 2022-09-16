using System;

namespace Chess.Pieces
{
    internal sealed class PieceEventArgs : EventArgs
    {
        public readonly Piece Piece;

        public PieceEventArgs(Piece piece)
        {
            Piece = piece;
        }
    }
}
