using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    class PieceEventArgs : EventArgs
    {
        public readonly Piece Piece;

        public PieceEventArgs(Piece piece)
        {
            Piece = piece;
        }
    }
}
