using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class Pawn : Piece
    {
        private bool hasMoved;
        private bool enPassant;
        public bool EnPassant { get { return enPassant; } set { enPassant = value; } }
        private int promotionSquareNumber;
        public Pawn(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Pawn, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            enPassant = false;
            moveSet = MoveSets.Pawn;
            promotionSquareNumber = team == Team.White ? 8 : 1;
            Value = team == Team.White ? 1 : -1;
            if (promotionSquareNumber == 8 && square.Number.digit != 2 || promotionSquareNumber == 1 && square.Number.digit != 7)
                hasMoved = true;
        }
        public Pawn(Pawn other, bool isRaw = false) : base(other.team, other.Square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : other.model;
            moves = other.CopyMoves();
            enPassant = other.enPassant;
            hasMoved = other.hasMoved;
            moveSet = other.moveSet;
            Value = other.Value;
            promotionSquareNumber = other.promotionSquareNumber;
        }
        public override int Update()
        {
            if (enPassant)
                enPassant = false;
            return base.Update();
        }
        public override void CheckPossibleMoves() //refactor
        {
            Team teamOnTheSquare;
            Square square;
            int direction = team == Team.White ? 1 : -1;

            if(!hasMoved) //double move
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
                        if (Owner.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                            moves.Add(moveToAdd);
                    }
                }
            }

            square = new Square(Square.Number.letter, Square.Number.digit + (1 * direction)); //regular move
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare == Team.Empty)
            {
                Move moveToAdd = new Move(Square, square, 'm');
                if (Owner.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                    moves.Add(moveToAdd);
            }
            //takes
            base.CheckPossibleMoves();

            square = new Square((char)(Square.Number.letter + 1), Square.Number.digit); //en passant to the right
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != team && teamOnTheSquare != Team.Empty)
            {
                if (Owner.GetPiece(square, out Piece piece) && piece is Pawn p && p.enPassant == true)
                {
                    Move moveToAdd = new Move(Square, new Square((char)(Square.Number.letter + 1), Square.Number.digit + (1 * direction)), 'p');
                    if (Owner.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }

            square = new Square((char)(Square.Number.letter - 1), Square.Number.digit); //en passant to the left
            teamOnTheSquare = Owner.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != Team.Empty && teamOnTheSquare != team)
            {
                if (Owner.GetPiece(square, out Piece piece) && piece is Pawn p && p.enPassant == true)
                {
                    Move moveToAdd = new Move(Square, new Square((char)(Square.Number.letter - 1), Square.Number.digit + (1 * direction)), 'p');
                    if (Owner.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }
        }
        public override void MovePiece(Move move)
        {         
            if (Math.Abs(Square.Number.digit - move.Latter.Number.digit) == 2)
                enPassant = true;
            Square = move.Latter;
            hasMoved = true;

            if (Square.Number.digit == promotionSquareNumber)
                Owner.OnPromotion(this);
            OnPieceMoved(new PieceMovedEventArgs(this, move));
        }
    }
}
