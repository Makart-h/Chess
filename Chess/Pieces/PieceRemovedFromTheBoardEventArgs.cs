using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    internal class PieceRemovedFromTheBoardEventArgs : EventArgs
    {
        public readonly Piece Piece;

        public PieceRemovedFromTheBoardEventArgs(Piece piece)
        {
            this.Piece = piece;
        }
    }
}
