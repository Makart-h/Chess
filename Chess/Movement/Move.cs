using Chess.Board;

namespace Chess.Movement;

internal readonly struct Move
{
    public readonly Square Former { get; }
    public readonly Square Latter { get; }
    public readonly char Description { get; }

    public Move(Square former, Square latter, char description)
    {
        Former = former;
        Latter = latter;
        Description = description;
    }
    public Move(Move other)
    {
        Former = other.Former;
        Latter = other.Latter;
        Description = other.Description;
    }
}
