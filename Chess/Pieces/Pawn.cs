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
        public Pawn(Team team, Square square, Texture2D rawTexture) : base(team, square)
        {
            model = new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Pawn, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            enPassant = false;
            moveSet = MoveSets.Pawn;
            promotionSquareNumber = team == Team.White ? 8 : 1;
            Value = 1;
        }
        public Pawn(Pawn other) : base(other.team, other.square)
        {
            model = other.model;
            moves = new List<Move>(other.moves);
            enPassant = other.enPassant;
            hasMoved = other.hasMoved;
            moveSet = other.moveSet;
            Value = other.Value;
            promotionSquareNumber = other.promotionSquareNumber;
        }
        public override void Update()
        {
            moves.Clear();
            CheckPossibleMoves();
            if (enPassant && Chessboard.Instance.ToMove == team)
                enPassant = false;
        }
        public override void CheckPossibleMoves() //refactor
        {
            Team teamOnTheSquare;
            Square square;
            int direction = team == Team.White ? 1 : -1;

            if(!hasMoved) //double move
            {
                square = new Square(this.square.Number.letter, this.square.Number.digit + 1 * direction);
                teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
                if (teamOnTheSquare == Team.Empty)
                {
                    square = new Square(this.Square.Number.letter, this.Square.Number.digit + 2 * direction);
                    teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
                    if (teamOnTheSquare == Team.Empty)
                    {
                        Move moveToAdd = new Move(this.square, square, "moves");
                        if (Chessboard.Instance.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                            moves.Add(moveToAdd);
                    }
                }
            }

            square = new Square(Square.Number.letter, Square.Number.digit + (1 * direction)); //regular move
            teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
            if (teamOnTheSquare == Team.Empty)
            {
                Move moveToAdd = new Move(this.square, square, "moves");
                if (Chessboard.Instance.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                    moves.Add(moveToAdd);
            }
            //takes
            base.CheckPossibleMoves();

            square = new Square((char)(Square.Number.letter + 1), Square.Number.digit); //en passant to the right
            teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != team && teamOnTheSquare != Team.Empty)
            {
                if (Chessboard.Instance.GetAPiece(square) is Pawn p && p.enPassant == true)
                {
                    Move moveToAdd = new Move(this.square, new Square((char)(Square.Number.letter + 1), this.square.Number.digit + (1 * direction)), "en passant");
                    if (Chessboard.Instance.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }

            square = new Square((char)(Square.Number.letter - 1), Square.Number.digit); //en passant to the left
            teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
            if (teamOnTheSquare != Team.Void && teamOnTheSquare != Team.Empty && teamOnTheSquare != team)
            {
                if (Chessboard.Instance.GetAPiece(square) is Pawn p && p.enPassant == true)
                {
                    Move moveToAdd = new Move(this.square, new Square((char)(Square.Number.letter - 1), this.square.Number.digit + (1 * direction)), "en passant");
                    if (Chessboard.Instance.GetKing(team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }
        }
        public override void MovePiece(Square square)
        {
            if (Math.Abs(this.square.Number.digit - square.Number.digit) == 2)
                enPassant = true;
            this.square = square;
            hasMoved = true;

            if (square.Number.digit == promotionSquareNumber)
                Chessboard.Instance.OnPromotion(this);
        }
    }
}
