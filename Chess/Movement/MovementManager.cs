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
    private readonly List<IInitiator> _initiators;
    private readonly Stack<IInitiator> _toRemove;
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
            IMovable movable = controller.GetPieceModel(e.Piece);
            if (movable != null)
            {
                Vector2 newPosition = Chessboard.Instance.ToCordsFromSquare(e.Move.Latter);
                if (controller is HumanController && e.Move.Description != MoveType.ParticipatesInCastling)
                {
                    movable.Position = newPosition;
                    _rapidMovement = true;
                }
                else
                {
                    _initiators.Add(new Initiator(newPosition, movable, MovementVelocity, OnDestinationReached));
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
        while (_toRemove.TryPop(out IInitiator i))
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
}
