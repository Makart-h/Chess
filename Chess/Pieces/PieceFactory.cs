using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;

namespace Chess.Pieces
{
    class PieceFactory
    {
        public static PieceFactory Instance;
        private readonly Texture2D piecesRawTexture;

        public PieceFactory(Texture2D piecesRawTexture)
        {
            PieceFactory.Instance = this;
            this.piecesRawTexture = piecesRawTexture;
        }
        public Piece CreateAPiece(char type, Square square)
        {
            return type switch
            {
                'p' => new Pawn(Team.Black, square, piecesRawTexture),
                'r' => new Rook(Team.Black, square, piecesRawTexture),
                'n' => new Knight(Team.Black, square, piecesRawTexture),
                'b' => new Bishop(Team.Black, square, piecesRawTexture),
                'k' => new King(Team.Black, square, piecesRawTexture),
                'q' => new Queen(Team.Black, square, piecesRawTexture),
                'P' => new Pawn(Team.White, square, piecesRawTexture),
                'R' => new Rook(Team.White, square, piecesRawTexture),
                'N' => new Knight(Team.White, square, piecesRawTexture),
                'B' => new Bishop(Team.White, square, piecesRawTexture),
                'K' => new King(Team.White, square, piecesRawTexture),
                'Q' => new Queen(Team.White, square, piecesRawTexture),
                _ => throw new NotImplementedException()
            };
        }
        public Piece CreateAPiece(PieceType type, Square square, Team team)
        {
            return type switch
            {
                PieceType.Queen => new Queen(team, square, piecesRawTexture),
                PieceType.Rook => new Queen(team, square, piecesRawTexture),
                PieceType.Pawn => new Queen(team, square, piecesRawTexture),
                PieceType.Knight => new Queen(team, square, piecesRawTexture),
                PieceType.Bishop => new Queen(team, square, piecesRawTexture),
                PieceType.King => new Queen(team, square, piecesRawTexture),
                _ => throw new NotImplementedException()
            };
        }
    }
}
