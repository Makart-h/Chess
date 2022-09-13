using System;
using Microsoft.Xna.Framework.Graphics;
using Chess.Board;
using Chess.Movement;

namespace Chess.Pieces
{
    class Pawn : Piece
    {
        private bool _hasMoved;
        private bool _enPassant;
        private readonly int _promotionSquareNumber;
        public bool EnPassant { get { return _enPassant; } set { _enPassant = value; } }

        public Pawn(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Pawn, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            _enPassant = false;
            _moveSet = MoveSets.Pawn;
            _promotionSquareNumber = team == Team.White ? 8 : 1;
            Value = team == Team.White ? 1 : -1;
            if (_promotionSquareNumber == 8 && square.Number.digit != 2 || _promotionSquareNumber == 1 && square.Number.digit != 7)
                _hasMoved = true;
        }
        public Pawn(Pawn other, bool isRaw = false) : base(other._team, other.Square, null)
        {
            IsRawPiece = isRaw;
            Model = IsRawPiece ? null : other.Model;
            moves = other.CopyMoves();
            _enPassant = other._enPassant;
            _hasMoved = other._hasMoved;
            _moveSet = other._moveSet;
            Value = other.Value;
            _promotionSquareNumber = other._promotionSquareNumber;
        }
        public override int Update()
        {
            if (_enPassant)
                _enPassant = false;
            return base.Update();
        }
        public override void CheckPossibleMoves()
        {
            Team teamOnTheSquare;
            Square square;
            int direction = _team == Team.White ? 1 : -1;

            // Double move that's available at the start position.
            if(!_hasMoved)
            {
                square = new Square(Square.Number.letter, Square.Number.digit + 1 * direction);
                teamOnTheSquare = Owner.IsSquareOccupied(square);
                if (teamOnTheSquare == Team.Empty)
                {
                    square = new Square(Square.Number.letter, Square.Number.digit + 2 * direction);
                    teamOnTheSquare = Owner.IsSquareOccupied(square);
                    if (teamOnTheSquare == Team.Empty)
                    {
                        Move moveToAdd = new Move(Square, square, 'm');
                        if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                            moves.Add(moveToAdd);
                    }
                }
            }

            // Regular move.
            square = new Square(Square.Number.letter, Square.Number.digit + (1 * direction));
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare == Team.Empty)
            {
                Move moveToAdd = new Move(Square, square, 'm');
                if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                    moves.Add(moveToAdd);
            }

            // Captures.
            base.CheckPossibleMoves();

            // En passant to the right.
            square = new Square((char)(Square.Number.letter + 1), Square.Number.digit);
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != _team && teamOnTheSquare != Team.Empty)
            {
                if (Owner.GetPiece(square, out Piece piece) && piece is Pawn p && p._enPassant == true)
                {
                    Move moveToAdd = new Move(Square, new Square((char)(Square.Number.letter + 1), Square.Number.digit + (1 * direction)), 'p');
                    if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }

            // En passant to the left.
            square = new Square((char)(Square.Number.letter - 1), Square.Number.digit);
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != Team.Empty && teamOnTheSquare != _team)
            {
                if (Owner.GetPiece(square, out Piece piece) && piece is Pawn p && p._enPassant == true)
                {
                    Move moveToAdd = new Move(Square, new Square((char)(Square.Number.letter - 1), Square.Number.digit + (1 * direction)), 'p');
                    if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }
        }
        public override void MovePiece(Move move)
        {         
            if (Math.Abs(Square.Number.digit - move.Latter.Number.digit) == 2)
                _enPassant = true;
            Square = move.Latter;
            _hasMoved = true;

            if (Square.Number.digit == _promotionSquareNumber)
                Owner.OnPromotion(this);
            OnPieceMoved(new PieceMovedEventArgs(this, move));
        }
    }
}
