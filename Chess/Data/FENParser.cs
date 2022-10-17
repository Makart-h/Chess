using Chess.Board;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Data;

internal static class FENParser
{
    private static readonly byte _boardWidth = 8;
    private static readonly byte _digitToIndexOffset = 1;
    private static readonly char _firstLetter = 'a';
    private static readonly char _rankSeparator = '/';
    private static Match ValidateString(string FEN)
    {
        RegexOptions regexOptions = RegexOptions.IgnoreCase;
        string regex = @"^(?'pieces'[/1-8prnbqk]{18,}) (?'toMove'[wb]) (?'castlingRights'-|[kq]{1,4}) (?'enPassant'-|[a-h]{1}[36]{1}) (?'halfMoves'\d{1,2}) (?'moveNo'\d+)$";
        return new Regex(regex, regexOptions).Match(FEN);
    }
    public static FENObject Parse(string FEN)
    {
        Match validatedString = ValidateString(FEN.Trim());

        if (!validatedString.Success)
            throw new ArgumentOutOfRangeException(nameof(FEN), "FEN string not in correct format!");

        try
        {
            (Piece[] whitePieces, Piece[] blackPieces) = ParsePieces(validatedString.Groups["pieces"].Value);
            Team toMove = ParseMoveOrder(validatedString.Groups["toMove"].Value);
            (CastlingRights white, CastlingRights black) = ParseCastlingRights(validatedString.Groups["castlingRights"].Value);
            Square? square = ParseEnPassant(validatedString.Groups["enPassant"].Value, toMove);
            int halfMoves = ParseIntValue(validatedString.Groups["halfMoves"].Value);
            int moveNo = ParseIntValue(validatedString.Groups["moveNo"].Value);

            return new FENObject(whitePieces, blackPieces, toMove, white, black, square, halfMoves, moveNo, validatedString.ToString());
        }
        catch (ArgumentException) { throw; }
    }
    private static (Piece[] white, Piece[] black) ParsePieces(string fenGroup)
    {
        List<Piece> white = new();
        List<Piece> black = new();
        int x = _boardWidth;
        int y = _firstLetter;
        foreach (char c in fenGroup)
        {
            if (c == _rankSeparator)
            {
                x--;
                y = _firstLetter;
            }
            else if (char.IsDigit(c))
            {
                y += int.Parse(c.ToString());
            }
            else
            {
                try
                {
                    Piece piece = PieceFactory.CreateAPiece(c, new Square((char)y, x));
                    if (piece.Team == Team.White)
                        white.Add(piece);
                    else
                        black.Add(piece);
                }
                catch (NotImplementedException)
                {
                    throw new ArgumentOutOfRangeException(nameof(c), $"'{c}' is not a valid character in FEN.");
                }
                finally
                {
                    y++;
                }
            }
        }
        return (white.ToArray(), black.ToArray());
    }
    private static Team ParseMoveOrder(string fenGroup) => fenGroup.Contains('w') ? Team.White : Team.Black;
    private static (CastlingRights white, CastlingRights black) ParseCastlingRights(string fenGroup)
    {
        CastlingRights white = CastlingRights.None;
        CastlingRights black = CastlingRights.None;

        foreach (char c in fenGroup)
        {
            switch (c)
            {
                case 'K':
                    white |= CastlingRights.KingSide;
                    break;
                case 'Q':
                    white |= CastlingRights.QueenSide;
                    break;
                case 'k':
                    black |= CastlingRights.KingSide;
                    break;
                case 'q':
                    black |= CastlingRights.QueenSide;
                    break;
            }
        }

        return (white, black);
    }
    private static Square? ParseEnPassant(string fenGroup, Team toMove)
    {
        try
        {
            Square square = new(fenGroup);
            int direction = toMove == Team.White ? -1 : 1;
            square.Transform((0, direction));
            return square;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
    private static int ParseIntValue(string fenGroup)
    {
        int value;
        try
        {
            value = int.Parse(fenGroup);
        }
        catch (Exception)
        {
            throw new ArgumentOutOfRangeException(nameof(fenGroup), $"'{fenGroup}' is not an integer.");
        }
        return value;
    }
    public static string ToFullFenString(Piece[] pieces, CastlingRights white, CastlingRights black, Team toMove, int halfMoves, int moves)
    {
        string shortFen = ToShortFenString(pieces, white, black, toMove);
        return shortFen + $" {(halfMoves == 0 ? "-" : halfMoves.ToString())} {(moves == 0 ? "-" : moves.ToString())}";
    }
    public static string ToLongFenString(Piece[] pieces, CastlingRights white, CastlingRights black, Team toMove, int halfMoves)
    {
        string shortFen = ToShortFenString(pieces, white, black, toMove);
        return shortFen + $" {(halfMoves == 0 ? "-" : halfMoves.ToString())}";
    }
    public static string ToShortFenString(Piece[] pieces, CastlingRights white, CastlingRights black, Team toMove)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(PiecesToString(pieces));
        sb.Append(' ');
        sb.Append(TeamTostring(toMove));
        sb.Append(' ');
        sb.Append(CastlingRightsToString(white, black));
        sb.Append(' ');
        sb.Append(EnPassantToString(pieces));

        return sb.ToString();
    }
    private static string PiecesToString(Piece[] pieces)
    {
        StringBuilder sb = new();
        int blanks = 0;
        for (int number = _boardWidth; number > 0; --number)
        {
            for (int letter = _firstLetter; letter < _firstLetter + _boardWidth; ++letter)
            {
                Piece p = pieces[((letter - _firstLetter) * _boardWidth) + number - _digitToIndexOffset];
                if (p != null)
                {
                    if (blanks > 0)
                    {
                        sb.Append(blanks);
                    }
                    try
                    {
                        sb.Append(PieceFactory.GetPieceCharacter(p));
                    }
                    catch(NotImplementedException)
                    {
                        throw new ArgumentOutOfRangeException(nameof(p), "Not a valid piece type!");
                    }
                    blanks = 0;
                }
                else
                {
                    blanks++;
                }
            }

            if (blanks != 0)
            {
                sb.Append(blanks);
                blanks = 0;
            }
            if (number != 1)
                sb.Append(_rankSeparator);
        }
        return sb.ToString();
    }
    private static string TeamTostring(Team toMove) => toMove == Team.White ? "w" : "b";
    private static string CastlingRightsToString(CastlingRights white, CastlingRights black)
    {
        StringBuilder sb = new StringBuilder();
        if ((white & CastlingRights.KingSide) != 0)
            sb.Append('K');
        if ((white & CastlingRights.QueenSide) != 0)
            sb.Append('Q');
        if ((black & CastlingRights.KingSide) != 0)
            sb.Append('k');
        if ((black & CastlingRights.QueenSide) != 0)
            sb.Append('q');

        if (sb.Length == 0)
            sb.Append('-');

        return sb.ToString();
    }
    private static string EnPassantToString(Piece[] pieces)
    {
        foreach (Piece piece in pieces)
        {
            if (piece is Pawn {EnPassant: true } p)
            {
                bool isCapturable = false;
                Square oneLeft = new(p.Square, (-1, 0));
                Square oneRight = new(p.Square, (1, 0));
                foreach (Square toCheck in new[] { oneLeft, oneRight })
                {
                    if (Square.Validate(toCheck))
                    {
                        Piece other = pieces[toCheck.Index];
                        if (other is Pawn)
                        {
                            isCapturable = true;
                            break;
                        }
                    }
                }
                if (isCapturable)
                {
                    int direction = p.Team == Team.White ? 1 : -1;
                    int digit = p.Square.Digit - (1 * direction);
                    return p.Square.Letter + digit.ToString();
                }
            }
        }
        return "-";
    }
}
