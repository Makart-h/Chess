using Chess.Board;
using Chess.Pieces.Info;
using Chess.Pieces;
using System;
using System.Collections.Generic;

namespace Chess.Movement;

internal class MoveGenerator
{
    private static readonly (int, int)[] _knightIterators;
    private static readonly (int, int)[] _rookIterators;
    private static readonly (int, int)[] _bishopIterators;
    private static readonly (int, int)[] _everyDirectionIterators;
    private static readonly MoveGeneratorOptions _defaultOptions;
    static MoveGenerator()
    {
        _knightIterators = new[] { (2, 1), (2, -1), (-1, -2), (1, -2), (-2, -1), (-2, 1), (1, 2), (-1, 2) };
        _rookIterators = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
        _bishopIterators = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };
        _everyDirectionIterators = new[] { (0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) };
        _defaultOptions = MoveGeneratorOptions.TakesIncluded | MoveGeneratorOptions.MovesIncluded;
    }
    public static List<Move> GenerateEveryMove(in Square initialSquare, Movesets set, IPieceOwner pieceOwner, bool friendlyFire = false)
    {
        List<Move> moves;
        MoveGeneratorOptions options = _defaultOptions;
        if (friendlyFire)
            options |= MoveGeneratorOptions.DefendsIncluded;
        // For a piece with a moveset of a pawn only the moves that lead to captures are considered.
        if (set == Movesets.Pawn)
        {
            moves = new(2);
            int direction = pieceOwner.GetTeamOnSquare(in initialSquare) == Team.White ? 1 : -1;
            (int, int)[] iterators = { (1, 1 * direction), (-1, 1 * direction) };
            foreach (var iterator in iterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options ^ MoveGeneratorOptions.MovesIncluded);
        }
        else if (set == Movesets.Knight)
        {
            moves = new(8);
            foreach (var iterator in _knightIterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options);
        }
        else if (set == Movesets.Bishop)
        {
            moves = new(13);
            foreach (var iterator in _bishopIterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options | MoveGeneratorOptions.InfiniteRange);
        }
        else if (set == Movesets.Rook)
        {
            moves = new(14);
            foreach (var iterator in _rookIterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options | MoveGeneratorOptions.InfiniteRange);
        }
        else if (set == Movesets.King)
        {
            moves = new(8);
            foreach (var iterator in _everyDirectionIterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options);
        }
        else
        {
            moves = new(27);
            foreach (var iterator in _everyDirectionIterators)
                GenerateMovesInDirection(moves, in initialSquare, iterator, pieceOwner, options | MoveGeneratorOptions.InfiniteRange);
        }
        return moves;
    }
    public static void GenerateMovesInDirection(List<Move> moves, in Square initialSquare, (int letter, int digit) squareIterator, IPieceOwner pieceOwner, MoveGeneratorOptions options)
    {
        if ((options & MoveGeneratorOptions.InfiniteRange) == 0)
        {
            Square nextSquare = new(in initialSquare, squareIterator);
            if (nextSquare.IsValid)
                GenerateMoveAndStop(moves, in initialSquare, in nextSquare, pieceOwner, options);
        }
        else
        {
            int numberOfIterations = GetNumberOfPossibleValidSquareIterations(in initialSquare, squareIterator);
            Square current = initialSquare;
            for (int i = 0; i < numberOfIterations; ++i)
            {
                current = current.Transform(squareIterator);
                if (GenerateMoveAndStop(moves, in initialSquare, in current, pieceOwner, options))
                    break;
            }
        }
    }
    public static bool GenerateMoveAndStop(List<Move> moves, in Square initialSquare, in Square current, IPieceOwner pieceOwner, MoveGeneratorOptions options)
    {
        Piece pieceOnTheWay = pieceOwner.GetPiece(in current);
        if (pieceOnTheWay != null)
        {
            if ((options & MoveGeneratorOptions.TakesIncluded) != 0 && pieceOnTheWay.Team != pieceOwner.GetTeamOnSquare(in initialSquare))
                moves.Add(new Move(in initialSquare, in current, MoveType.Takes));
            else if ((options & MoveGeneratorOptions.DefendsIncluded) != 0)
                moves.Add(new Move(in initialSquare, in current, MoveType.Defends));
            return true;
        }
        else if ((options & MoveGeneratorOptions.MovesIncluded) != 0)
        {
            moves.Add(new Move(in initialSquare, in current, MoveType.Moves));
        }
        return false;
    }
    public static int GetNumberOfPossibleValidSquareIterations(in Square initialSquare, (int letter, int digit) squareIterator)
    {
        int letterValidIterations = int.MaxValue;
        int digitValidIterations = int.MaxValue;

        if (squareIterator.letter < 0)
            letterValidIterations = (initialSquare.Letter - Square.MinLetter) / -squareIterator.letter;
        else if (squareIterator.letter > 0)
            letterValidIterations = (Square.MaxLetter - initialSquare.Letter) / squareIterator.letter;
        if (squareIterator.digit < 0)
            digitValidIterations = (initialSquare.Digit - Square.MinDigit) / -squareIterator.digit;
        else if (squareIterator.digit > 0)
            digitValidIterations = (Square.MaxDigit - initialSquare.Digit) / squareIterator.digit;

        if (letterValidIterations == int.MaxValue && digitValidIterations == int.MaxValue)
            return 0;
        else
            return Math.Min(letterValidIterations, digitValidIterations);
    }
}
