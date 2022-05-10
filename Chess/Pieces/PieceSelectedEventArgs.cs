using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    class PieceSelectedEventArgs : EventArgs
    {
        public Piece piece;

        public PieceSelectedEventArgs(Piece piece)
        {
            this.piece = piece;
        }
    }
}
