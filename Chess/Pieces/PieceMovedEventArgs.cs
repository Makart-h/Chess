using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Movement;

namespace Chess.Pieces
{
    class PieceMovedEventArgs : EventArgs
    {
        public Piece Piece;
        public Move Move;

        public PieceMovedEventArgs(Piece piece, Move move)
        {
            Piece = piece;
            Move = move;
        }
    }
}
