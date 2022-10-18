using Chess.Board;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal interface IPieceOwner
{
    public King GetKing(Team team);
    public bool TryGetPiece(Square square, out Piece piece);
    public Team GetTeamOnSquare(Square square);
    public void OnPromotion(Piece piece);
}
