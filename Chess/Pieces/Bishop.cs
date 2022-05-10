using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class Bishop : Piece
    {
        public Bishop(Team team, Square square, Texture2D rawTexture) : base(team, square)
        {
            model = new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Bishop, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Bishop;
            Value = 3;
        }
        public Bishop(Bishop other) : base(other.team, other.square)
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
        public override void MovePiece(Move move)
        {
            OnPieceMoved(new PieceMovedEventArgs(this, move));
            this.square = move.Latter;
        }
    }
}
