using Chess.Board;

namespace Chess.Positions.Evaluators;

internal struct BoardControlInfo
{
    private static readonly double s_squareValue = 0.01;
    private static readonly double s_smallCenterValue = 0.1;
    private static readonly double s_centerValue = 0.3;
    private static readonly (char min, char max) s_centerLetter;
    private static readonly (int min, int max) s_centerDigit;
    private static readonly (char min, char max) s_smallCenterLetter;
    private static readonly (int min, int max) s_smallCenterDigit;
    private int _centerControl;
    private int _smallCenterControl;
    private int _squaresControl;
    static BoardControlInfo()
    {
        s_centerLetter = ('d', 'e');
        s_centerDigit = (4, 5);
        s_smallCenterLetter = ('c', 'f');
        s_smallCenterDigit = (3, 6);
    }
    public double Value { get => CalculateValue(); }
    public void IncreaseControls(int sign, Square square)
    {
        _squaresControl += sign;

        if (IsInCenter(square))
            _centerControl += sign;
        else if (IsInSmallCenter(square))
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
    private static bool IsInCenter(Square square) => square.Letter >= s_centerLetter.min && square.Letter <= s_centerLetter.max && square.Digit >= s_centerDigit.min && square.Digit <= s_centerDigit.max;
    private static bool IsInSmallCenter(Square square) => square.Letter >= s_smallCenterLetter.min && square.Letter <= s_smallCenterLetter.max && square.Digit >= s_smallCenterDigit.min && square.Digit <= s_smallCenterDigit.max;
}
