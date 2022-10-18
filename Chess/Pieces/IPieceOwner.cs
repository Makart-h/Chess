using Chess.Board;
using Chess.Pieces.Info;

namespace Chess.Pieces;

internal interface IPieceOwner
{
    public King GetKing(Team team);
    public Piece GetPiece(Square square);
    public Team GetTeamOnSquare(Square square);
    public void OnPromotion(Piece piece);
}
