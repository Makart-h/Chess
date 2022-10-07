namespace Chess.Pieces.Info;

internal sealed class PromotionEventArgs
{
    public Pawn PromotedPawn { get; init; }
    public PieceType TypeToBePromotedInto { get; init; }

    public PromotionEventArgs(Pawn promotedPawn, PieceType typeToBePromotedInto)
    {
        PromotedPawn = promotedPawn;
        TypeToBePromotedInto = typeToBePromotedInto;
    }
}
