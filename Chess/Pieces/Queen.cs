using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal sealed class Queen : Piece
{
    public Queen(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Queen, isRaw)
    {
        _moveset = Movesets.Queen;
        Value = team == Team.White ? 9 : -9;
    }
    public Queen(Queen other, bool isRaw = false) : base(other, isRaw) { }
    public override void CheckPossibleMoves()
    {
        base.CheckPossibleMoves();
    }
    public override void MovePiece(Move move)
    {
        base.MovePiece(move);
    }
}
