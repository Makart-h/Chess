using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class Knight : Piece
    {
        public Knight(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Knight, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Knight;
            Value = team == Team.White ? 3 : -3;
        }
        public Knight(Knight other, bool isRaw = false) : base(other.team, other.square)
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
            square = move.Latter;
        }
    }
}
