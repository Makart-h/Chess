using Chess.Board;
using Chess.Movement;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Pieces
{
    internal sealed class Knight : Piece
    {
        public Knight(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Knight, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            _moveSet = MoveSets.Knight;
            Value = team == Team.White ? 3 : -3;
        }
        public Knight(Knight other, bool isRaw = false) : base(other._team, other.Square, null)
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
