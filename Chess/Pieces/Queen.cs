using Microsoft.Xna.Framework.Graphics;
using Chess.Board;
using Chess.Movement;


namespace Chess.Pieces
{
    internal sealed class Queen : Piece
    {
        public Queen(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Queen, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            _moveSet = MoveSets.Queen;
            Value = team == Team.White ? 9 : -9;
        }
        public Queen(Queen other, bool isRaw = false) : base(other._team, other.Square, null)
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
