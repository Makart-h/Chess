using Microsoft.Xna.Framework.Graphics;
using Chess.Board;
using Chess.Movement;

namespace Chess.Pieces
{
    class Bishop : Piece
    {
        public Bishop(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Bishop, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            _moveSet = MoveSets.Bishop;
            Value = team == Team.White ? 3 : -3;
        }
        public Bishop(Bishop other, bool isRaw = false) : base(other._team, other.Square)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : other.Model;
            moves = other.CopyMoves();
            _moveSet = other._moveSet;
            Value = other.Value;
        }
        public override void CheckPossibleMoves()
        {
            base.CheckPossibleMoves();
        }
        public override void MovePiece(Move move)
        {
            OnPieceMoved(new PieceMovedEventArgs(this, move));
            Square = move.Latter;
        }
    }
}
