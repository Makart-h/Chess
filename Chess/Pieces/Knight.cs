using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal sealed class Knight : Piece
{
    public Knight(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Knight, isRaw)
    {
        _moveset = Movesets.Knight;
        Value = team == Team.White ? 3 : -3;
    }
    public Knight(Knight other, bool isRaw = false) : base(other, isRaw) { }
    public override void CheckPossibleMoves()
    {
        base.CheckPossibleMoves();
    }
    public override void MovePiece(Move move)
    {
        base.MovePiece(move);
    }
}
