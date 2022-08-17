﻿using System;
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
        public Rook(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.Rook, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            moveSet = MoveSets.Rook;
            Value = team == Team.White ? 5 : -5;
        }
        public Rook(Rook other, bool isRaw = false) : base(other.team, other.square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : other.model;
            moves = other.CopyMoves();
            HasMoved = other.HasMoved;
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
            HasMoved = true;
        }
    }
}
