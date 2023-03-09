using System;

namespace Chess.Pieces.Info;

internal sealed class PromotionEventArgs : EventArgs
{
    public Pawn PromotedPawn { get; init; }
    public PieceType TypeToBePromotedInto { get; init; }

    public PromotionEventArgs(Pawn promotedPawn, PieceType typeToBePromotedInto)
    {
        PromotedPawn = promotedPawn;
        TypeToBePromotedInto = typeToBePromotedInto;
    }
}
