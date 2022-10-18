using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal sealed class Rook : Piece
{
    public bool HasMoved { get; private set; }
    public Rook(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Rook, isRaw)
    {
        _moveset = Movesets.Rook;
        Value = team == Team.White ? 5 : -5;
    }
    public Rook(Rook other, bool isRaw = false) : base(other, isRaw)
    {
        HasMoved = other.HasMoved;
    }
    public override void MovePiece(in Move move)
    {
        base.MovePiece(in move);
        HasMoved = true;
    }
}
