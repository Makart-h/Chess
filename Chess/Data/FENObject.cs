using Chess.Board;
using Chess.Pieces;
using System.Collections.Generic;

namespace Chess.Data
{
    internal sealed class FENObject
    {
        public Piece[] WhitePieces { get; private set; }
        public Piece[] BlackPieces { get; private set; }
        public Piece[] AllPieces { get; private set; }
        public Team TeamToMove { get; private set; }
        public CastlingRights WhiteCastling { get; private set; }
        public CastlingRights BlackCastling { get; private set; }
        public Square? EnPassantSquare { get; private set; }
        public int HalfMoves { get; private set; }
        public int MoveNo { get; private set; }

        public FENObject(Piece[] whitePieces, Piece[] blackPieces, Team teamToMove, CastlingRights whiteCastling, CastlingRights blackCastling, Square? enPassantSquare, int halfMoves, int moveNo)
        {
            WhitePieces = whitePieces;
            BlackPieces = blackPieces;
            List<Piece> temp = new List<Piece>(whitePieces);
            temp.AddRange(blackPieces);
            AllPieces = temp.ToArray();
            TeamToMove = teamToMove;
            WhiteCastling = whiteCastling;
            BlackCastling = blackCastling;
            EnPassantSquare = enPassantSquare;
            HalfMoves = halfMoves;
            MoveNo = moveNo;
        }
    }
}
