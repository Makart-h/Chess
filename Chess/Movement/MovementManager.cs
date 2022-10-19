using Chess.AI;
using Chess.Board;
using Chess.Graphics;
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
    private void OnPieceMoved(object sender, PieceMovedEventArgs e)
    {
        if (e.Piece.Owner is Controller controller)
        {
            DrawableObject drawablePiece = controller.GetDrawablePiece(e.Piece);
            if (drawablePiece != null)
            {
                Vector2 newPosition = Chessboard.Instance.ToCordsFromSquare(e.Move.Latter);
                if (controller is HumanController && e.Move.Description != MoveType.ParticipatesInCastling)
                {
                    drawablePiece.MoveObject(newPosition - drawablePiece.Position);
                    _rapidMovement = true;
                }
                else
                {
                    _initiators.Add(new Initiator(newPosition, drawablePiece, OnDestinationReached));
                }
            }
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
    public static List<Move> GenerateEveryMove(Square initialSquare, Movesets set, IPieceOwner pieceOwner, bool friendlyFire = false)
    {
        List<Move> moves;
        // For a piece with a moveset of a pawn only the moves that lead to captures are considered.
        if (set == Movesets.Pawn)
        {
            moves = new(2);
            int direction = pieceOwner.GetTeamOnSquare(initialSquare) == Team.White ? 1 : -1;
            GenerateMovesInDirection(moves, initialSquare, (1, 1 * direction), pieceOwner, infiniteRange: false, movesIncluded: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 1 * direction), pieceOwner, infiniteRange: false, movesIncluded: false, friendlyFire: friendlyFire);
        }
        else if (set == Movesets.Knight)
        {
            moves = new(4);
            GenerateMovesInDirection(moves, initialSquare, (2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (2, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, -2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-2, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-2, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 2), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
        }  
        else if (set == Movesets.Bishop)
        {
            moves = new(7);
            GenerateMovesInDirection(moves, initialSquare, (1, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, -1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, -1), pieceOwner, friendlyFire: friendlyFire);
        }
        else if (set == Movesets.Rook)
        {
            moves = new(7);
            GenerateMovesInDirection(moves, initialSquare, (0, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (0, -1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, 0), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 0), pieceOwner, friendlyFire: friendlyFire);
        }
        else if (set == Movesets.King)
        {
            moves = new(4);
            GenerateMovesInDirection(moves, initialSquare, (0, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (0, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 0), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, -1), pieceOwner, infiniteRange: false, friendlyFire: friendlyFire);
        }
        else
        {
            moves = new(14);
            GenerateMovesInDirection(moves, initialSquare, (1, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, -1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, -1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (0, 1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (0, -1), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (1, 0), pieceOwner, friendlyFire: friendlyFire);
            GenerateMovesInDirection(moves, initialSquare, (-1, 0), pieceOwner, friendlyFire: friendlyFire);
        }
        return moves;
    }
    public static void GenerateMovesInDirection(List<Move> moves, Square initialSquare, (int letter, int digit) cordsIterator, IPieceOwner pieceOwner, bool infiniteRange = true, bool movesIncluded = true, bool friendlyFire = false)
    {
        Square current = initialSquare;
        Piece pieceOnTheWay;
        do
        {
            current.Transform(cordsIterator);
            if (!Square.Validate(current))
                break;

            pieceOnTheWay = pieceOwner.GetPiece(current);
            if (pieceOnTheWay != null)
            {
                if (pieceOnTheWay.Team != pieceOwner.GetTeamOnSquare(initialSquare))
                    moves.Add(new Move(initialSquare, current, MoveType.Takes));
                else if (friendlyFire)
                    moves.Add(new Move(initialSquare, current, MoveType.Defends));
                break;
            }
            else if (movesIncluded)
            {
                moves.Add(new Move(initialSquare, current, MoveType.Moves));
            }
        } while (infiniteRange);
    }
}
