using System;
using Chess.Pieces;

namespace Chess.Board
{
  internal class Position
  {
    private Dictionary<Square, LightPiece> pieces;
    private Team activeTeam;
    private int halfMoves;
    
    public Position(Position other, Move move)
    {
     foreach(var square in other.pieces.Keys)
     {
      //create new LightPiece in LightPieceFactory
     }
     activeTeam = other.activeTeam == Team.White ? Team.Black : Team.White;
     halfMoves = other.halfMoves;
     ApplyMove(move);
    }
    private void ApplyMove(Move move)
    {
      //move a LightPiece
    }
    private Update()
    {
     foreach(var item in pieces.Keys)
     {
      pieces[item]?.Update(); 
     }
    }
  }
}
