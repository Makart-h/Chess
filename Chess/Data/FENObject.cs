using Chess.Board;
using Chess.Pieces;
using Chess.Pieces.Info;
using System.Collections.Generic;

namespace Chess.Data;

internal readonly struct FENObject
{
    public Piece[] WhitePieces { get; init; }
    public Piece[] BlackPieces { get; init; }
    public Piece[] AllPieces { get; init; }
    public Team TeamToMove { get; init; }
    public CastlingRights WhiteCastling { get; init; }
    public CastlingRights BlackCastling { get; init; }
    public Square? EnPassantSquare { get; init; }
    public int HalfMoves { get; init; }
    public int MoveNo { get; init; }
    public string FEN { get; init; }

    public FENObject(Piece[] whitePieces, Piece[] blackPieces, Team teamToMove, CastlingRights whiteCastling, CastlingRights blackCastling, Square? enPassantSquare, int halfMoves, int moveNo, string fEN)
    {
        WhitePieces = whitePieces;
        BlackPieces = blackPieces;
        List<Piece> temp = new(whitePieces);
        temp.AddRange(blackPieces);
        AllPieces = temp.ToArray();
        TeamToMove = teamToMove;
        WhiteCastling = whiteCastling;
        BlackCastling = blackCastling;
        EnPassantSquare = enPassantSquare;
        HalfMoves = halfMoves;
        MoveNo = moveNo;
        FEN = fEN;
    }
}
