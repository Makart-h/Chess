using Chess.Movement;
using System;

namespace Chess.Pieces
{
    internal sealed class PieceMovedEventArgs : EventArgs
    {
        public readonly Piece Piece;
        public readonly Move Move;
        public PieceMovedEventArgs(Piece piece, Move move)
        {
            Piece = piece;
            Move = move;
        }
    }
}
