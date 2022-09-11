using Chess.Board;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    internal interface IPieceOwner
    {
        public King GetKing(Team team);
        public bool GetPiece(Square square, out Piece piece);
        public Team IsSquareOccupied(Square square);
        public bool ArePiecesFacingEachOther(Piece first, Piece second);
        public void OnPromotion(Piece piece);
    }
}
