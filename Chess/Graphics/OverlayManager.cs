using Chess.Board;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Graphics;

internal sealed class OverlayManager : IDrawableProvider, IDisposable
{
    private static OverlayManager s_instance;
    public static OverlayManager Instance { get => s_instance ?? throw new NullReferenceException(); }
    private readonly IOverlayFactory _overlayFactory;
    private readonly List<IMovableDrawable> _selections;
    private readonly List<IMovableDrawable> _moves;
    private bool _showMoves;
    private bool _isDisposed;
    private OverlayManager(IOverlayFactory overlayFactory)
    {
        _selections = new();
        _moves = new();
        _showMoves = false;
        _overlayFactory = overlayFactory;
        SubscribeToEvents();
    }
    private void SubscribeToEvents()
    {
        Piece.PieceSelected += OnPieceSelected;
        Piece.PieceDeselected += OnPieceDeselected;
        Piece.PieceMoved += OnPieceMoved;
        Chessboard.BoardInverted += OnBoardInverted;
        King.Check += OnCheck;
        MovementManager.MovementConcluded += OnMovementConcluded;
    }
    private void UnsubscribeFromEvents()
    {
        Piece.PieceSelected -= OnPieceSelected;
        Piece.PieceDeselected -= OnPieceDeselected;
        Piece.PieceMoved -= OnPieceMoved;
        Chessboard.BoardInverted -= OnBoardInverted;
        King.Check -= OnCheck;
        MovementManager.MovementConcluded -= OnMovementConcluded;
    }
    public static void Create(IOverlayFactory overlayFactory)
    {
        if (s_instance != null)
            throw new InvalidOperationException("Instance already created!");

        s_instance = new OverlayManager(overlayFactory);
    }
    public IEnumerable<IDrawable> GetDrawableObjects()
    {
        if(_isDisposed)
            return null;

        List<IDrawable> overlays = new(_selections);
        if (_showMoves)
            overlays.AddRange(_moves);
        return overlays;
    }
    private void OnBoardInverted(object sender, EventArgs e)
    {
        foreach (IMovable overlay in _moves)
        {
            overlay.Position = MovementManager.RecalculateVector(overlay.Position);
        }
        foreach (IMovable overlay in _selections)
        {
            overlay.Position = MovementManager.RecalculateVector(overlay.Position);
        }
    }
    private void OnPieceSelected(object sender, PieceEventArgs e)
    {
        _selections.AddRange(_overlayFactory.CreateOverlays(sender, e));
    }
    private void OnPieceDeselected(object sender, EventArgs e) => _selections.Clear();
    private void OnPieceMoved(object sender, PieceMovedEventArgs e)
    {
        List<IMovableDrawable> overlays = new(_overlayFactory.CreateOverlays(sender, e));
        if(overlays.Count > 0)
        {
            _showMoves = false;
            _moves.Clear();
            _moves.AddRange(overlays);
        }
    }
    private void OnCheck(object sender, EventArgs e)
    {
        _moves.AddRange(_overlayFactory.CreateOverlays(sender, e));
    }
    private void OnMovementConcluded(object sender, EventArgs e) => _showMoves = true;
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _selections.Clear();
        _moves.Clear();
        UnsubscribeFromEvents();
        s_instance = null;
    }
}
