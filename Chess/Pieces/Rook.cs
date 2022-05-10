using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class Rook : Piece
    {
        public bool HasMoved { get; private set; }
        public Rook(Team team, Square square, Texture2D rawTexture) : base(team, square)
        {
            model = new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Rook, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Rook;
            Value = 5;
        }
        public Rook(Rook other) : base(other.team, other.square)
        {
            model = other.model;
            moves = new List<Move>(other.moves);
            HasMoved = other.HasMoved;
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
            HasMoved = true;
        }
    }
}
