using System;

namespace Chess.Positions;

internal readonly struct Evaluation : IComparable, IComparable<Evaluation>, IEquatable<Evaluation>
{
    public double Value { get; init; }
    public int Depth { get; init; }
    public string Path { get; init; }

    public Evaluation(double value, int depth, string path)
    {
        Value = value;
        Depth = depth;
        Path = path;
    }
    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        else if (obj is Evaluation e)
            return CompareTo(e);
        else
            throw new ArgumentException("Object is not an Evaluation!");
    }
    public int CompareTo(Evaluation other)
    {
        int valueComparision = Value.CompareTo(other.Value);
        if (valueComparision == 0)
            return Depth.CompareTo(other.Depth);
        else
            return valueComparision;
    }
    public bool Equals(Evaluation other)
    {
        return Value.Equals(other.Value) && Depth.Equals(other.Depth);
    }
    public override bool Equals(object obj)
    {
        return obj is Evaluation evaluation && Equals(evaluation);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Depth);
    }
    public static bool operator <(Evaluation left, Evaluation right)
    {
        return left.CompareTo(right) < 0;
    }
    public static bool operator <=(Evaluation left, Evaluation right)
    {
        return left.CompareTo(right) <= 0;
    }
    public static bool operator >(Evaluation left, Evaluation right)
    {
        return left.CompareTo(right) > 0;
    }
    public static bool operator >=(Evaluation left, Evaluation right)
    {
        return left.CompareTo(right) >= 0;
    }
    public static bool operator ==(Evaluation left, Evaluation right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Evaluation left, Evaluation right)
    {
        return !left.Equals(right);
    }
}
