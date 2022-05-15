using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Chess.Pieces;
using Chess.Board;

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
            Match validatedString = ValidateString(FEN);

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
            List<Piece> white = new List<Piece>();
            List<Piece> black = new List<Piece>();
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
                        Piece piece = PieceFactory.Instance.CreateAPiece(c, new Square((char)y, x));
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
    }
}
