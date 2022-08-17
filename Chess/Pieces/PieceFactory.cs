using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using System.Reflection;

namespace Chess.Pieces
{
    static class PieceFactory
    {
        private static bool isInitilized;
        private static Texture2D piecesRawTexture;

        public static void Initilize(Texture2D piecesRawTexture)
        {
            PieceFactory.piecesRawTexture = piecesRawTexture;
            isInitilized = true;
        }
        public static Piece CreateAPiece(char type, Square square, bool isRaw = false)
        {
            if (!isInitilized)
                throw new TypeInitializationException("Factory not initilized!", null);

            return type switch
            {
                'p' => new Pawn(Team.Black, square, piecesRawTexture, isRaw),
                'r' => new Rook(Team.Black, square, piecesRawTexture, isRaw),
                'n' => new Knight(Team.Black, square, piecesRawTexture, isRaw),
                'b' => new Bishop(Team.Black, square, piecesRawTexture, isRaw),
                'k' => new King(Team.Black, square, piecesRawTexture, isRaw),
                'q' => new Queen(Team.Black, square, piecesRawTexture, isRaw),
                'P' => new Pawn(Team.White, square, piecesRawTexture, isRaw),
                'R' => new Rook(Team.White, square, piecesRawTexture, isRaw),
                'N' => new Knight(Team.White, square, piecesRawTexture, isRaw),
                'B' => new Bishop(Team.White, square, piecesRawTexture, isRaw),
                'K' => new King(Team.White, square, piecesRawTexture, isRaw),
                'Q' => new Queen(Team.White, square, piecesRawTexture, isRaw),
                _ => throw new NotImplementedException()
            };
        }
        public static Piece CreateAPiece(PieceType type, Square square, Team team, bool isRaw = false)
        {
            if (!isInitilized)
                throw new TypeInitializationException("Factory not initilized!", null);

            return type switch
            {
                PieceType.Queen => new Queen(team, square, piecesRawTexture, isRaw),
                PieceType.Rook => new Rook(team, square, piecesRawTexture, isRaw),
                PieceType.Pawn => new Pawn(team, square, piecesRawTexture, isRaw),
                PieceType.Knight => new Knight(team, square, piecesRawTexture, isRaw),
                PieceType.Bishop => new Bishop(team, square, piecesRawTexture, isRaw),
                PieceType.King => new King(team, square, piecesRawTexture, isRaw),
                _ => throw new NotImplementedException()
            };
        }
        public static Piece CopyAPiece(Piece piece, IPieceOwner owner, bool isRaw = false)
        {
            if (!isInitilized)
                throw new TypeInitializationException("Factory not initilized!", null);

            Type type = piece.GetType();
            ConstructorInfo ci = type.GetConstructor(new[] { type, isRaw.GetType() });

            var copy = ci?.Invoke(new object[] { piece, isRaw });
            if (copy is Piece p)
            {
                p.Owner = owner;
                return p;
            }
            return null;
        }
    }
}
