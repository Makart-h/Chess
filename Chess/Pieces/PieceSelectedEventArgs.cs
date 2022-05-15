using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    class PieceSelectedEventArgs : EventArgs
    {
        public readonly Piece piece;

        public PieceSelectedEventArgs(Piece piece)
        {
            this.piece = piece;
        }
    }
}
