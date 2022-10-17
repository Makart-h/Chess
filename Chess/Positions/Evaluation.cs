using System;
using System.Collections.Generic;

namespace Chess.Positions;

internal readonly struct Evaluation : IEquatable<Evaluation>
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
    public static Evaluation Max(IEnumerable<Evaluation> enumerable, bool standardComparison)
    {
        Evaluation? maxValue = null;
        foreach (Evaluation evaluation in enumerable)
        {
            if(maxValue == null || evaluation.CompareTo(maxValue.Value, standardComparison) == 1)
                maxValue = evaluation;
        }
        return maxValue.Value;
    }
    public int CompareTo(Evaluation other, bool standardComparison)
    {
        int sign = standardComparison ? 1 : -1;
        int valueComparision = Value.CompareTo(other.Value);
        if (valueComparision == 0)
        {
            sign = Value == 1000 * sign ? -1 : 1;
            return Depth.CompareTo(other.Depth) * sign;
        }
        else
            return valueComparision * sign;
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
    public static bool operator ==(Evaluation left, Evaluation right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Evaluation left, Evaluation right)
    {
        return !left.Equals(right);
    }
}
