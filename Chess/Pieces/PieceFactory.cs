using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Chess.Board;

namespace Chess.Pieces
{
    static class PieceFactory
    {
        private static bool _isInitilized;
        private static Texture2D _piecesRawTexture;

        public static void Initilize(Texture2D piecesRawTexture)
        {
            PieceFactory._piecesRawTexture = piecesRawTexture;
            _isInitilized = true;
        }
        public static Piece CreateAPiece(char type, Square square, bool isRaw = false)
        {
            if (!_isInitilized)
                throw new TypeInitializationException("Factory not initilized!", null);

            return type switch
            {
                'p' => new Pawn(Team.Black, square, _piecesRawTexture, isRaw),
                'r' => new Rook(Team.Black, square, _piecesRawTexture, isRaw),
                'n' => new Knight(Team.Black, square, _piecesRawTexture, isRaw),
                'b' => new Bishop(Team.Black, square, _piecesRawTexture, isRaw),
                'k' => new King(Team.Black, square, _piecesRawTexture, isRaw),
                'q' => new Queen(Team.Black, square, _piecesRawTexture, isRaw),
                'P' => new Pawn(Team.White, square, _piecesRawTexture, isRaw),
                'R' => new Rook(Team.White, square, _piecesRawTexture, isRaw),
                'N' => new Knight(Team.White, square, _piecesRawTexture, isRaw),
                'B' => new Bishop(Team.White, square, _piecesRawTexture, isRaw),
                'K' => new King(Team.White, square, _piecesRawTexture, isRaw),
                'Q' => new Queen(Team.White, square, _piecesRawTexture, isRaw),
                _ => throw new NotImplementedException()
            };
        }
        public static Piece CreateAPiece(PieceType type, Square square, Team team, bool isRaw = false)
        {
            if (!_isInitilized)
                throw new TypeInitializationException("Factory not initilized!", null);

            return type switch
            {
                PieceType.Queen => new Queen(team, square, _piecesRawTexture, isRaw),
                PieceType.Rook => new Rook(team, square, _piecesRawTexture, isRaw),
                PieceType.Pawn => new Pawn(team, square, _piecesRawTexture, isRaw),
                PieceType.Knight => new Knight(team, square, _piecesRawTexture, isRaw),
                PieceType.Bishop => new Bishop(team, square, _piecesRawTexture, isRaw),
                PieceType.King => new King(team, square, _piecesRawTexture, isRaw),
                _ => throw new NotImplementedException()
            };
        }
        public static Piece CopyAPiece(Piece piece, IPieceOwner owner, bool isRaw = false)
        {
            if (!_isInitilized)
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
        public static char GetPieceType(Piece piece)
        {
            if(piece is King k)
            {
                if (k.Team == Team.White)
                    return 'K';
                else
                    return 'k';
            }
            else if (piece is Queen q)
            {
                if (q.Team == Team.White)
                    return 'Q';
                else
                    return 'q';
            }
            else if (piece is Bishop b)
            {
                if (b.Team == Team.White)
                    return 'B';
                else
                    return 'b';
            }
            else if (piece is Knight n)
            {
                if (n.Team == Team.White)
                    return 'N';
                else
                    return 'n';
            }
            else if (piece is Rook r)
            {
                if (r.Team == Team.White)
                    return 'R';
                else
                    return 'r';
            }
            else if (piece is Pawn p)
            {
                if (p.Team == Team.White)
                    return 'P';
                else
                    return 'p';
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
