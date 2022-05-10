using System;
using System.Collections.Generic;
using Chess.Pieces;
using Chess.Movement;

namespace Chess.Graphics
{
    static class OverlayManager
    {
        private static readonly List<SquareOverlay> selections;
        private static readonly List<SquareOverlay> moves;

        static OverlayManager()
        {
            selections = new List<SquareOverlay>();
            moves = new List<SquareOverlay>();
            Piece.PieceSelected += OnPieceSelected;
            Piece.PieceDeselected += OnPieceDeselected;
            Piece.PieceMoved += OnPieceMoved;
            King.Check += OnCheck;
        }
        public static List<DrawableObject> GetOverlays()
        {
            List<DrawableObject> overlays = new List<DrawableObject>(selections);
            overlays.AddRange(moves);
            return overlays;
        }
        public static void OnPieceSelected(object sender, PieceSelectedEventArgs args)
        {
            Piece piece = args.piece;
            selections.Add(new SquareOverlay(SquareOverlayType.Selected, piece.Square));
            foreach (var item in piece.Moves)
            {
                switch (item.Description)
                {
                    default:
                    case "moves":
                        selections.Add(new SquareOverlay(SquareOverlayType.CanMove, item.Latter));
                        break;
                    case "takes":
                    case "en passant":
                        selections.Add(new SquareOverlay(SquareOverlayType.CanTake, item.Latter));
                        break;
                }
            }
        }
        public static void OnPieceDeselected(object sender, EventArgs args)
        {
            selections.Clear();
        }
        public static void OnPieceMoved(object sender, PieceMovedEventArgs args)
        {
            Move move = args.Move;
            moves.Clear();
            moves.Add(new SquareOverlay(SquareOverlayType.MovedFrom, move.Former));
            moves.Add(new SquareOverlay(SquareOverlayType.MovedTo, move.Latter));
        }
        public static void OnCheck(object sender, EventArgs args)
        {
            if (sender is Piece p)
            {
                moves.Add(new SquareOverlay(SquareOverlayType.Check, p.Square));
            }
        }
    }
}
