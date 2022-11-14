using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Chess.Graphics;

internal class SquareOverlayFactory : IOverlayFactory
{
    private readonly Texture2D _overlaysTexture;
    public SquareOverlayFactory(Texture2D overlaysTexture)
    {
        _overlaysTexture = overlaysTexture;
    }
    public IEnumerable<IMovableDrawable> CreateOverlays(object sender, EventArgs e)
    {
        List<IMovableDrawable> overlays = e switch
        {
            PieceEventArgs pieceEventArgs => CreatePieceOverlays(pieceEventArgs),
            PieceMovedEventArgs pieceMoved => CreatePieceMovedOverlays(pieceMoved),
            EventArgs => CreateCheckOverlay(sender),
            _ => throw new NotImplementedException()
        };
        return overlays;
    }
    private List<IMovableDrawable> CreatePieceOverlays(PieceEventArgs e)
    {
        Piece piece = e.Piece;
        List<IMovableDrawable> overlays = new() { new SquareOverlay(_overlaysTexture, SquareOverlayType.Selected, piece.Square) };
        foreach (Move move in piece.Moves)
        {
            switch (move.Description)
            {
                default:
                case MoveType.Moves:
                    overlays.Add(new SquareOverlay(_overlaysTexture, SquareOverlayType.CanMove, move.Latter));
                    break;
                case MoveType.Takes:
                case MoveType.EnPassant:
                    overlays.Add(new SquareOverlay(_overlaysTexture, SquareOverlayType.CanTake, move.Latter));
                    break;
            }
        }
        return overlays;
    }
    private  List<IMovableDrawable> CreatePieceMovedOverlays(PieceMovedEventArgs e)
    {
        List<IMovableDrawable> overlays = new();
        if (e.Move.Description != MoveType.ParticipatesInCastling)
        {
            Move move = e.Move;
            overlays.Add(new SquareOverlay(_overlaysTexture, SquareOverlayType.MovedFrom, move.Former));
            overlays.Add(new SquareOverlay(_overlaysTexture, SquareOverlayType.MovedTo, move.Latter));
        }
        return overlays;
    }
    private List<IMovableDrawable> CreateCheckOverlay(object sender)
    {
        List<IMovableDrawable> overlays = new();
        if (sender is Piece p)
        {
            overlays.Add(new SquareOverlay(_overlaysTexture, SquareOverlayType.Check, p.Square));
        }
        return overlays;
    }
}
