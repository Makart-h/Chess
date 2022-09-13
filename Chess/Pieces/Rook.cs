using Microsoft.Xna.Framework.Graphics;
using Chess.Board;
using Chess.Movement;

namespace Chess.Pieces
{
    class Rook : Piece
    {
        public bool HasMoved { get; private set; }
        public Rook(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Rook, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            _moveSet = MoveSets.Rook;
            Value = team == Team.White ? 5 : -5;
        }
        public Rook(Rook other, bool isRaw = false) : base(other._team, other.Square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : other.Model;
            moves = other.CopyMoves();
            HasMoved = other.HasMoved;
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
            HasMoved = true;
        }
    }
}
