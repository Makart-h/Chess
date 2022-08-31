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
        public Bishop(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Bishop, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Bishop;
            Value = team == Team.White ? 3 : -3;
        }
        public Bishop(Bishop other, bool isRaw = false) : base(other.team, other.square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : other.model;
            moves = other.CopyMoves();
            moveSet = other.moveSet;
            Value = other.Value;
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
