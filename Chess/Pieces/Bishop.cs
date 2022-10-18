using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal sealed class Bishop : Piece
{
    public Bishop(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Bishop, isRaw)
    {     
        _moveset = Movesets.Bishop;
        Value = team == Team.White ? 3 : -3;
    }
    public Bishop(Bishop other, bool isRaw = false) : base(other, isRaw) { }
}
