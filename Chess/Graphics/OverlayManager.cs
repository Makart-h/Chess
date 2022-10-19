using Chess.Board;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Chess.Graphics;

internal sealed class OverlayManager : IDrawableProvider, IDisposable
{
    private static OverlayManager s_instance;
    public static OverlayManager Instance
    {
        get
        {
            return s_instance ?? throw new ArgumentNullException();
        }
        private set { s_instance = value; }
    }
    private readonly List<SquareOverlay> _selections;
    private readonly List<SquareOverlay> _moves;
    private bool _showMoves;
    private bool _isDisposed;
    public Texture2D OverlaysTexture { get; init; }
    private OverlayManager(Texture2D overlaysTexture)
    {
        _selections = new();
        _moves = new();
        _showMoves = false;
        OverlaysTexture = overlaysTexture;
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
    public static void Create(Texture2D overlaysTexture)
    {
        if (s_instance != null)
            throw new InvalidOperationException("Instance already created!");

        s_instance = new OverlayManager(overlaysTexture);
    }
    public DrawableObject[] GetDrawableObjects()
    {
        if(_isDisposed)
            return null;

        List<DrawableObject> overlays = new(_selections);
        if (_showMoves)
            overlays.AddRange(_moves);
        return overlays.ToArray();
    }
    private void OnBoardInverted(object sender, EventArgs e)
    {
        foreach (DrawableObject overlay in _moves)
        {
            overlay.RecalculatePosition();
        }
        foreach (DrawableObject overlay in _selections)
        {
            overlay.RecalculatePosition();
        }
    }
    private void OnPieceSelected(object sender, PieceEventArgs e)
    {
        Piece piece = e.Piece;
        _selections.Add(new SquareOverlay(SquareOverlayType.Selected, piece.Square));
        foreach (Move move in piece.Moves)
        {
            switch (move.Description)
            {
                default:
                case MoveType.Moves:
                    _selections.Add(new SquareOverlay(SquareOverlayType.CanMove, move.Latter));
                    break;
                case MoveType.Takes:
                case MoveType.EnPassant:
                    _selections.Add(new SquareOverlay(SquareOverlayType.CanTake, move.Latter));
                    break;
            }
        }
    }
    private void OnPieceDeselected(object sender, EventArgs e) => _selections.Clear();
    private void OnPieceMoved(object sender, PieceMovedEventArgs e)
    {
        if (e.Move.Description != MoveType.ParticipatesInCastling)
        {
            _showMoves = false;
            Move move = e.Move;
            _moves.Clear();
            _moves.Add(new SquareOverlay(SquareOverlayType.MovedFrom, move.Former));
            _moves.Add(new SquareOverlay(SquareOverlayType.MovedTo, move.Latter));
        }
    }
    private void OnCheck(object sender, EventArgs e)
    {
        if (sender is Piece p)
        {
            _moves.Add(new SquareOverlay(SquareOverlayType.Check, p.Square));
        }
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
