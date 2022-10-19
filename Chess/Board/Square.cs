using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chess.Board;

internal struct Square : IEquatable<Square>
{
    private static readonly Regex s_regex;
    private static readonly char s_minLetter;
    private static readonly char s_maxLetter;
    private static readonly int s_minDigit;
    private static readonly int s_maxDigit;
    static Square()
    {
        RegexOptions options = RegexOptions.IgnoreCase;
        string regex = @"^(?'letter'[a-h]{1})(?'number'[1-8]{1})$";
        s_regex = new Regex(regex, options);
        s_minDigit = 1;
        s_maxDigit = 8;
        s_minLetter = 'a';
        s_maxLetter = 'h';
    }
    private int _index;
    private char _letter;
    private int _digit;
    public readonly char Letter { get => _letter; }
    public readonly int Digit { get => _digit; }
    public readonly int Index { get => _index; }
    public readonly bool IsValid { get => _letter >= s_minLetter && _letter <= s_maxLetter && _digit >= s_minDigit && _digit <= s_maxDigit; }
    public Square(char letter, int digit)
    {
        _letter = char.ToLower(letter);
        _digit = digit;
        _index = ((_letter - s_minLetter) * s_maxDigit) + _digit - 1;
    }
    public Square(Square other, (int x, int y) iterator)
    {
        _letter = (char)(other._letter + iterator.x);
        _digit = other._digit + iterator.y;
        _index = other._index + iterator.x * s_maxDigit + iterator.y;
    }
    public Square(string square)
    {
        var match = s_regex.Match(square);
        if (match.Success)
        {
            _letter = char.ToLower(match.Groups["letter"].Value.First());

            if (int.TryParse(match.Groups["number"].Value, out int digit))
            {
                _digit = digit;
                _index = ((_letter - s_minLetter) * s_maxDigit) + _digit - 1;
                return;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(square), "Not a valid chess square!");
    }
    public static bool IsLightSquare(Square square)
    {
        int letterInt = square._letter - 'a' + 1;
        int digit = square._digit;
        if (letterInt % 2 == 0)
            return digit % 2 != 0;
        else
            return digit % 2 == 0;
    }
    public void Transform((int letter, int digit) iterator)
    {
        _letter = (char)(Letter + iterator.letter);
        _digit += iterator.digit;
        _index += iterator.letter * s_maxDigit + iterator.digit;
    }
    public static bool operator ==(Square first, Square second) => first._index == second._index;
    public static (int x, int y) operator -(Square first, Square second) => (first._letter - second._letter, first._digit - second._digit);
    public static bool Validate(Square square) => square._letter >= s_minLetter && square._letter <= s_maxLetter && square._digit >= s_minDigit && square._digit <= s_maxDigit;
    public static bool operator !=(Square first, Square second) => !(first == second);
    public override readonly bool Equals(object obj) => (obj is Square s) && this == s;
    public readonly bool Equals(Square other) => this == other;
    public override readonly int GetHashCode() => HashCode.Combine(_letter.GetHashCode(), _digit.GetHashCode());
    public override readonly string ToString() => _letter + _digit.ToString();
    public readonly void Deconstruct(out char letter, out int digit)
    {
        letter = _letter;
        digit = _digit;
    } 
}