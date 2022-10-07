using Chess.Board;
using System.Linq;

namespace Chess.Positions.Evaluators;

internal struct BoardControlInfo
{
    private static readonly double s_squareValue = 0.02;
    private static readonly double s_smallCenterValue = 0.1;
    private static readonly double s_centerValue = 0.2;
    private static readonly Square[] s_smallCenter;
    private static readonly Square[] s_center;
    private int _centerControl;
    private int _smallCenterControl;
    private int _squaresControl;
    static BoardControlInfo()
    {
        s_center = new[] { new Square("E4"), new Square("E5"), new Square("D4"), new Square("D5") };
        s_smallCenter = new[] { new Square("E3"), new Square("E6"), new Square("D3"), new Square("D6"),
        new Square("C6"), new Square("C5"), new Square("C4"), new Square("C3"),
        new Square("F6"), new Square("F5"), new Square("F4"), new Square("F3") };
    }
    public double Value { get => CalculateValue(); }
    public void IncreaseControls(int sign, Square square)
    {
        _squaresControl += sign;

        if (s_center.Contains(square))
            _centerControl += sign;
        else if (s_smallCenter.Contains(square))
            _smallCenterControl += sign;
    }
    private double CalculateValue()
    {
        double value = 0;
        value += _squaresControl * s_squareValue;
        value += _centerControl * s_centerValue;
        value += _smallCenterControl * s_smallCenterValue;
        return value;
    }
}
