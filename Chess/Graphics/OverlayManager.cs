using System;
using System.Collections.Generic;
using Chess.Pieces;
using Chess.Board;
using Chess.Movement;

namespace Chess.Graphics
{
    static class OverlayManager
    {
        private static readonly List<SquareOverlay> _selections;
        private static readonly List<SquareOverlay> _moves;
        static OverlayManager()
        {
            _selections = new List<SquareOverlay>();
            _moves = new List<SquareOverlay>();
            SubscribeToEvents();          
        }
        private static void SubscribeToEvents()
        {
            Piece.PieceSelected += OnPieceSelected;
            Piece.PieceDeselected += OnPieceDeselected;
            Piece.PieceMoved += OnPieceMoved;
            Chessboard.BoardInverted += OnBoardInverted;
            King.Check += OnCheck;
        }
        public static List<DrawableObject> GetOverlays()
        {
            List<DrawableObject> overlays = new List<DrawableObject>(_selections);
            overlays.AddRange(_moves);
            return overlays;
        }
        public static void OnBoardInverted(object sender, EventArgs e)
        {
            foreach(SquareOverlay overlay in _moves)
            {
                overlay.RecalculatePosition();
            }
        }
        public static void OnPieceSelected(object sender, PieceEventArgs e)
        {
            Piece piece = e.Piece;
            _selections.Add(new SquareOverlay(SquareOverlayType.Selected, piece.Square));
            foreach (Move move in piece.Moves)
            {
                switch (move.Description)
                {
                    default:
                    case 'm':
                        _selections.Add(new SquareOverlay(SquareOverlayType.CanMove, move.Latter));
                        break;
                    case 'x':
                    case 'p':
                        _selections.Add(new SquareOverlay(SquareOverlayType.CanTake, move.Latter));
                        break;
                }
            }
        }
        public static void OnPieceDeselected(object sender, EventArgs e) => _selections.Clear();
        public static void OnPieceMoved(object sender, PieceMovedEventArgs e)
        {
            if (e.Move.Description != 'c')
            {
                Move move = e.Move;
                _moves.Clear();
                _moves.Add(new SquareOverlay(SquareOverlayType.MovedFrom, move.Former));
                _moves.Add(new SquareOverlay(SquareOverlayType.MovedTo, move.Latter));
            }
        }
        public static void OnCheck(object sender, EventArgs e)
        {
            if (sender is Piece p)
            {
                _moves.Add(new SquareOverlay(SquareOverlayType.Check, p.Square));
            }
        }
    }
}
