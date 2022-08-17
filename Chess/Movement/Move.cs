using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Pieces;

namespace Chess.Movement
{
    class Move
    {
        public Square Former { get; private set; }
        public Square Latter { get; private set; }
        public string Description { get; private set; }
        
        public Move(Square former, Square latter, string description)
        {
            Former = former;
            Latter = latter;
            Description = description;
        }
        public Move(Move other)
        {
            Former = other.Former;
            Latter = other.Latter;
            Description = other.Description;
        }
        public static List<(MoveSets, List<Move>)> GenerateEveryMove(Piece piece, MoveSets set)
        {
            List<(MoveSets, List<Move>)> movesGroupedByDirection = new List<(MoveSets, List<Move>)>();

            if((set & MoveSets.Vertical) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(piece, (s => new Square(s.Number.letter, s.Number.digit + 1)))));
                movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(piece, (s => new Square(s.Number.letter, s.Number.digit - 1)))));
            }
            if ((set & MoveSets.Horizontal) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter+1), s.Number.digit)))));
                movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter-1), s.Number.digit)))));
            }
            if ((set & MoveSets.Diagonal) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit + 1)))));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit - 1)))));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit + 1)))));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit - 1)))));
            }
            if ((set & MoveSets.Knight) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 2), s.Number.digit + 1)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 2), s.Number.digit - 1)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit - 2)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit - 2)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 2), s.Number.digit - 1)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 2), s.Number.digit + 1)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit + 2)), false)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit + 2)), false)));
            }
            if((set & MoveSets.Pawn) != 0) //only takes
            {
                int direction = piece.Team == Team.White ? 1 : -1;
                movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit + 1*direction)), false, "moves")));
                movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit + 1*direction)), false, "moves")));
            }
            if((set & MoveSets.King) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square(s.Number.letter, s.Number.digit + 1)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square(s.Number.letter, s.Number.digit - 1)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit + 1)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + 1), s.Number.digit - 1)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit + 1)), false)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter - 1), s.Number.digit - 1)), false)));
            }
            return movesGroupedByDirection;
        }
        public static List<Move> GenerateMovesInADirection(Piece piece, Func<Square, Square> cordsIterator, bool infiniteRange = true, string excludeMove = "none")
        {
            List<Move> moves = new List<Move>();
            Square current = piece.Square;
            do
            {
                current = cordsIterator(current);
                Team occupiedSquare = Chessboard.Instance.IsSquareOccupied(current);
                Chessboard.Instance.GetAPiece(current, out Piece pieceOnTheWay);
                if (occupiedSquare != Team.Empty && pieceOnTheWay != piece)
                {
                    if (excludeMove != "takes" && occupiedSquare != piece.Team && occupiedSquare != Team.Void)
                        moves.Add(new Move(piece.Square, current, "takes"));
                    break;
                }
                else if (excludeMove != "moves")
                {
                    moves.Add(new Move(piece.Square, current, "moves"));
                }
            } while (infiniteRange);
            return moves;
        }
    }
}
