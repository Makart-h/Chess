using Chess.Board;
using Chess.Graphics;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Model = Chess.Graphics.Model;

namespace Chess.Pieces;

internal static class PieceFactory
{
    private static bool _isInitilized;
    private static Texture2D _piecesRawTexture;
    public static Texture2D PiecesRawTexture
    {
        get
        {
            CheckInitialization();
            return _piecesRawTexture;
        }
    }
    public static int PieceTextureWidth;
    // Taking colors of the pieces into consideration.
    private const int _numberOfDifferentPieces = 12;

    public static void Initilize(Texture2D piecesRawTexture)
    {
        _piecesRawTexture = piecesRawTexture;
        PieceTextureWidth = _piecesRawTexture.Width / _numberOfDifferentPieces;
        _isInitilized = true;
    }
    private static void CheckInitialization()
    {
        if (!_isInitilized)
            throw new TypeInitializationException("Factory not initilized!", null);
    }
    public static IMovableDrawable CreatePieceModel(Piece piece)
    {
        Chessboard board = Chessboard.Instance;
        int texturePosX = PieceTextureWidth * (int)piece.Type + (PiecesRawTexture.Width / 2 * ((byte)piece.Team & 1));
        Rectangle textureRect = new(texturePosX, 0, PieceTextureWidth, PieceTextureWidth);
        Vector2 position = board.ToCordsFromSquare(piece.Square);
        Rectangle destinationRect = new((int)position.X, (int)position.Y, board.SquareSideLength, board.SquareSideLength);
        MovableDrawable drawable = new(position, PiecesRawTexture, textureRect, destinationRect);
        return drawable;
    }
    public static Piece CreateAPiece(char type, Square square, bool isRaw = false)
    {
        CheckInitialization();

        return type switch
        {
            'p' => new Pawn(Team.Black, square, isRaw),
            'r' => new Rook(Team.Black, square, isRaw),
            'n' => new Knight(Team.Black, square, isRaw),
            'b' => new Bishop(Team.Black, square, isRaw),
            'k' => new King(Team.Black, square, isRaw),
            'q' => new Queen(Team.Black, square, isRaw),
            'P' => new Pawn(Team.White, square, isRaw),
            'R' => new Rook(Team.White, square, isRaw),
            'N' => new Knight(Team.White, square, isRaw),
            'B' => new Bishop(Team.White, square, isRaw),
            'K' => new King(Team.White, square, isRaw),
            'Q' => new Queen(Team.White, square, isRaw),
            _ => throw new NotImplementedException()
        };
    }
    public static Piece CreateAPiece(PieceType type, Square square, Team team, bool isRaw = false)
    {
        CheckInitialization();

        return type switch
        {
            PieceType.Queen => new Queen(team, square, isRaw),
            PieceType.Rook => new Rook(team, square, isRaw),
            PieceType.Pawn => new Pawn(team, square, isRaw),
            PieceType.Knight => new Knight(team, square, isRaw),
            PieceType.Bishop => new Bishop(team, square, isRaw),
            PieceType.King => new King(team, square, isRaw),
            _ => throw new NotImplementedException()
        };
    }
    public static Piece CopyAPiece(Piece piece, IPieceOwner owner, bool isRaw = false)
    {
        CheckInitialization();

        Piece copy = piece switch
        {
            King k => new King(k, isRaw),
            Queen q => new Queen(q, isRaw),
            Bishop b => new Bishop(b, isRaw),
            Knight n => new Knight(n, isRaw),
            Rook r => new Rook(r, isRaw),
            Pawn p => new Pawn(p, isRaw),
            _ => throw new NotImplementedException()
        };
        copy.Owner = owner;
        return copy;
    }
    public static char GetPieceCharacter(Piece piece)
    {
        if (piece is King k)
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
    public static char GetPieceCharacter(PieceType pieceType, Team team)
    {
        return (pieceType, team) switch
        {
            (PieceType.Queen, Team.White) => 'Q',
            (PieceType.Queen, Team.Black) => 'q',
            (PieceType.King, Team.White) => 'K',
            (PieceType.King, Team.Black) => 'k',
            (PieceType.Bishop, Team.White) => 'B',
            (PieceType.Bishop, Team.Black) => 'b',
            (PieceType.Knight, Team.White) => 'N',
            (PieceType.Knight, Team.Black) => 'n',
            (PieceType.Rook, Team.White) => 'R',
            (PieceType.Rook, Team.Black) => 'r',
            (PieceType.Pawn, Team.White) => 'P',
            (PieceType.Pawn, Team.Black) => 'p',
            _ => throw new NotImplementedException()
        };
    }
}
