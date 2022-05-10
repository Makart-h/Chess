using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class Queen : Piece
    {
        public Queen(Team team, Square square, Texture2D rawTexture) : base(team, square)
        {
            model = new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Queen, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Queen;
            Value = 9;
        }
        public Queen(Queen other) : base(other.team, other.square)
        {
            model = other.model;
            moves = new List<Move>(other.moves);
            moveSet = other.moveSet;
            Value = other.Value;
        }
        public override void Update()
        {
            moves.Clear();
            CheckPossibleMoves();
        }
        public override void CheckPossibleMoves()
        {
            base.CheckPossibleMoves();
        }
        public override void MovePiece(Square square)
        {
            this.square = square;
        }
    }
}
