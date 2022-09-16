using System.Collections.Generic;
using Chess.Board;
using Chess.Pieces;

namespace Chess.Movement
{
    internal sealed class Move
    {
        public Square Former { get; private set; }
        public Square Latter { get; private set; }
        public char Description { get; private set; }
        
        public Move(Square former, Square latter, char description)
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
        public static (MoveSets set, Move[] moves)[] GenerateEveryMove(Square initialSquare, MoveSets set, IPieceOwner pieceOwner, bool friendlyFire = false, Piece piece = null)
        {
            List<(MoveSets, Move[])> movesGroupedByDirection = new List<(MoveSets, Move[])>();

            if((set & MoveSets.Vertical) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(initialSquare, (0, 1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(initialSquare, (0, -1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
            }
            if ((set & MoveSets.Horizontal) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(initialSquare, (1, 0), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(initialSquare, (-1, 0), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
            }
            if ((set & MoveSets.Diagonal) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (1, 1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (1, -1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (-1, 1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (-1, -1), pieceOwner, friendlyFire: friendlyFire, piece: piece)));
            }
            if ((set & MoveSets.Knight) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (2, -1), pieceOwner,  infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-2, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
            }
            // For a piece with a moveset of a pawn only the moves that lead to captures are considered.
            if((set & MoveSets.Pawn) != 0)
            {

                int direction = (piece != null ? piece.Team : pieceOwner.IsSquareOccupied(initialSquare)) == Team.White ? 1 : -1;
                movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(initialSquare, (1, 1*direction), pieceOwner, infiniteRange: false, "moves", friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(initialSquare, (-1, 1*direction), pieceOwner, infiniteRange: false, "moves", friendlyFire: friendlyFire, piece: piece)));
            }
            if((set & MoveSets.King) != 0)
            {
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (0, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (0, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
                movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, piece: piece)));
            }
            return movesGroupedByDirection.ToArray();
        }
        public static Move[] GenerateMovesInADirection(Square initialSquare, (int letter, int digit) cordsIterator, IPieceOwner pieceOwner, bool infiniteRange = true, string excludeMove = "none", bool friendlyFire = false, Piece piece = null)
        {
            List<Move> moves = new List<Move>();
            Square current = initialSquare;
            Team team = piece != null ? piece.Team : pieceOwner.IsSquareOccupied(initialSquare);
            do
            {
                current.Transform(cordsIterator);
                if (!Square.Validate(current))
                    break;

                Team occupiedSquare = pieceOwner.IsSquareOccupied(current);
                pieceOwner.GetPiece(current, out Piece pieceOnTheWay);
                if (occupiedSquare != Team.Empty && pieceOnTheWay != piece)
                {
                    if (excludeMove != "takes" && (occupiedSquare != team || friendlyFire))
                        moves.Add(new Move(initialSquare, current, 'x'));
                    break;
                }
                else if (excludeMove != "moves")
                {
                    moves.Add(new Move(initialSquare, current, 'm'));
                }
            } while (infiniteRange);
            return moves.ToArray();
        }
    }
}
