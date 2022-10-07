using Chess.AI;
using Chess.Board;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Chess.Movement;

internal sealed class MovementManager : IDisposable
{
    private static MovementManager s_instance;
    public static MovementManager Instance { get => s_instance ??= new MovementManager(); }
    private readonly List<Initiator> _initiators;
    private readonly Stack<Initiator> _toRemove;
    public static EventHandler MovementConcluded;
    private bool _rapidMovement;
    private bool _isDisposed;
    public float MovementVelocity { get; private set; }
    private MovementManager()
    {
        _initiators = new();
        _toRemove = new();
        MovementVelocity = 0.6f;
        Piece.PieceMoved += OnPieceMoved;
        _rapidMovement = false;
    }
    private void OnPieceMoved(object sender, PieceMovedEventArgs args)
    {
        Vector2 newPosition = Chessboard.Instance.ToCordsFromSquare(args.Move.Latter);
        if (args.Piece.Owner is HumanController && args.Move.Description != 'c')
        {
            args.Piece.MoveObject(newPosition - args.Piece.Position);
            _rapidMovement = true;
        }
        else
        {
            _initiators.Add(new Initiator(newPosition, args.Piece, OnDestinationReached));
        }
    }
    private void OnDestinationReached(object sender, EventArgs args)
    {
        if (sender is Initiator i)
        {
            i.Dispose();
            _toRemove.Push(i);
        }
    }
    public void Update(GameTime gameTime)
    {    
        foreach (Initiator initiator in _initiators)
            initiator.Update(gameTime);
        bool initiatorRemovedOnThisUpdate = TryRemoveInitiators();
        TryConcludeMovement(initiatorRemovedOnThisUpdate);
    }
    private bool TryRemoveInitiators()
    {
        bool initiatorRemovedOnThisUpdate = false;
        while (_toRemove.TryPop(out Initiator i))
        {
            _initiators.Remove(i);
            initiatorRemovedOnThisUpdate = true;
        }
        return initiatorRemovedOnThisUpdate;
    }
    private void TryConcludeMovement(bool initiatorRemovedOnThisUpdate)
    {
        if (_initiators.Count == 0 && (initiatorRemovedOnThisUpdate || _rapidMovement))
        {
            OnMovementConcluded(EventArgs.Empty);
            _rapidMovement = false;
        }
    }
    public void Dispose()
    {
        if (_isDisposed)
            return;

        Piece.PieceMoved -= OnPieceMoved;
        foreach (var initiator in _initiators)
        {
            initiator.Dispose();
        }
        _initiators.Clear();
        _toRemove.Clear();
        s_instance = null;
        _isDisposed = true;
    }
    public static Vector2 RecalculateVector(Vector2 vector)
    {
        float width = Chessboard.Instance.SquareSideLength * (Chessboard.NumberOfSquares - 1);
        float height = Chessboard.Instance.SquareSideLength * (Chessboard.NumberOfSquares - 1);
        return new Vector2(width - vector.X, height - vector.Y);
    }
    private static void OnMovementConcluded(EventArgs e) => MovementConcluded?.Invoke(null, e);
    public static (MoveSets set, Move[] moves)[] GenerateEveryMove(Square initialSquare, MoveSets set, IPieceOwner pieceOwner, bool friendlyFire = false, Piece pieceToIgnore = null)
    {
        List<(MoveSets, Move[])> movesGroupedByDirection = new();

        if ((set & MoveSets.Diagonal) != 0)
        {
            movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (1, 1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (1, -1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (-1, 1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Diagonal, GenerateMovesInADirection(initialSquare, (-1, -1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
        }
        if (set == MoveSets.Rook || set == MoveSets.Queen)
        {
            movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(initialSquare, (0, 1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Vertical, GenerateMovesInADirection(initialSquare, (0, -1), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(initialSquare, (1, 0), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Horizontal, GenerateMovesInADirection(initialSquare, (-1, 0), pieceOwner, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
        }
        // For a piece with a moveset of a pawn only the moves that lead to captures are considered.
        else if ((set & MoveSets.Pawn) != 0)
        {
            int direction = (pieceToIgnore != null ? pieceToIgnore.Team : pieceOwner.GetTeamOnSquare(initialSquare)) == Team.White ? 1 : -1;
            movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(initialSquare, (1, 1 * direction), pieceOwner, infiniteRange: false, "moves", friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Pawn, GenerateMovesInADirection(initialSquare, (-1, 1 * direction), pieceOwner, infiniteRange: false, "moves", friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
        }
        else if ((set & MoveSets.Knight) != 0)
        {
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (2, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-2, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.Knight, GenerateMovesInADirection(initialSquare, (-1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
        }
        else if ((set & MoveSets.King) != 0)
        {
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (0, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (0, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
            movesGroupedByDirection.Add((MoveSets.King, GenerateMovesInADirection(initialSquare, (-1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire, pieceToIgnore: pieceToIgnore)));
        }
        return movesGroupedByDirection.ToArray();
    }
    public static Move[] GenerateMovesInADirection(Square initialSquare, (int letter, int digit) cordsIterator, IPieceOwner pieceOwner, bool infiniteRange = true, string excludeMove = "none", bool friendlyFire = false, Piece pieceToIgnore = null)
    {
        List<Move> moves = new();
        Square current = initialSquare;
        Team team = pieceToIgnore != null ? pieceToIgnore.Team : pieceOwner.GetTeamOnSquare(initialSquare);
        do
        {
            if (cordsIterator == (0, 0))
                break;

            current.Transform(cordsIterator);
            if (!current.IsValid)
                break;

            if (pieceOwner.TryGetPiece(current, out Piece pieceOnTheWay) && pieceOnTheWay != pieceToIgnore)
            {
                if (excludeMove != "takes" && (pieceOnTheWay.Team != team || friendlyFire))
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
