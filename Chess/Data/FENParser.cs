using Chess.Board;
using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Data
{
    internal static class FENParser
    {
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
                throw new ArgumentException("FEN string not in correct format!");

            try
            {
                (Piece[] whitePieces, Piece[] blackPieces) = ParsePieces(validatedString.Groups["pieces"].Value);
                Team toMove = ParseMoveOrder(validatedString.Groups["toMove"].Value);
                (CastlingRights white, CastlingRights black) = ParseCastlingRights(validatedString.Groups["castlingRights"].Value);
                Square? square = ParseEnPassant(validatedString.Groups["enPassant"].Value, toMove);
                int halfMoves = ParseIntValue(validatedString.Groups["halfMoves"].Value);
                int moveNo = ParseIntValue(validatedString.Groups["moveNo"].Value);

                return new FENObject(whitePieces, blackPieces, toMove, white, black, square, halfMoves, moveNo);
            }
            catch (ArgumentException) { throw; }
        }
        private static (Piece[] white, Piece[] black) ParsePieces(string fenGroup)
        {
            List<Piece> white = new();
            List<Piece> black = new();
            int x = 8;
            int y = 'A';
            foreach (char c in fenGroup)
            {
                if (c == '/')
                {
                    x--;
                    y = 'A';
                }
                else if (char.IsDigit(c))
                {
                    y += c - '0';
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
                    catch (NotImplementedException e)
                    {
                        throw new ArgumentException(e.Message);
                    }
                    finally
                    {
                        y++;
                    }
                }
            }
            return (white.ToArray(), black.ToArray());
        }
        private static Team ParseMoveOrder(string fenGroup) => fenGroup.Contains("w") ? Team.White : Team.Black;
        private static (CastlingRights white, CastlingRights black) ParseCastlingRights(string fenGroup)
        {
            CastlingRights white = CastlingRights.None;
            CastlingRights black = CastlingRights.None;

            foreach (var c in fenGroup)
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
            Square? square = null;

            if (!fenGroup.Contains('-'))
            {
                if (fenGroup.Length != 2)
                    throw new ArgumentException($"{fenGroup.Length} is not a valid number of characters to describe a square!");

                int direction = toMove == Team.White ? -1 : 1;
                square = new Square(fenGroup[0], fenGroup[1] - '0' + direction);
            }

            return square;
        }
        private static int ParseIntValue(string fenGroup)
        {
            int value;
            try
            {
                value = int.Parse(fenGroup);
            }
            catch(Exception e)
            {
                throw new ArgumentException(e.Message);
            }
            return value;
        }
        public static string ToFullFenString(Dictionary<Square, Piece> pieces, CastlingRights white, CastlingRights black, Team toMove, int halfMoves, int moves)
        {
            string shortFen = ToShortFenString(pieces, white, black, toMove);
            return shortFen + $" {(halfMoves == 0 ? "-" : halfMoves.ToString())} {(moves == 0 ? "-" : moves.ToString())}";
        }
        public static string ToLongFenString(Dictionary<Square, Piece> pieces, CastlingRights white, CastlingRights black, Team toMove, int halfMoves)
        {
            string shortFen = ToShortFenString(pieces, white, black, toMove);
            return shortFen + $" {(halfMoves == 0 ? "-" : halfMoves.ToString())}";
        }
        public static string ToShortFenString(Dictionary<Square, Piece> pieces, CastlingRights white, CastlingRights black, Team toMove)
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
        private static string PiecesToString(Dictionary<Square, Piece> pieces)
        {
            StringBuilder sb = new();
            int blanks = 0;
            for (int number = 8; number >= 1; --number)
            {
                for (int letter = 'a'; letter <= 'h'; ++letter)
                {
                    Piece p = pieces[new Square((char)letter, number)];
                    if (p != null)
                    {
                        if (blanks > 0)
                        {
                            sb.Append(blanks);
                        }
                        sb.Append(PieceFactory.GetPieceType(p));
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
                    sb.Append('/');
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
        private static string EnPassantToString(Dictionary<Square, Piece> pieces)
        {
            foreach (Piece piece in pieces.Values)
            {
                if (piece is Pawn p && p.EnPassant == true)
                {
                    int direction = p.Team == Team.White ? 1 : -1;
                    int digit = p.Square.Digit - (1 * direction);
                    return p.Square.Letter + digit.ToString();
                }               
            }
            return "-";
        }
    }
}
