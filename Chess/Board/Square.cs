﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chess.Board;

internal struct Square
{
    private static readonly Regex _regex;
    static Square()
    {
        RegexOptions options = RegexOptions.IgnoreCase;
        string regex = @"^(?'letter'[a-h]{1})(?'number'[1-8]{1})$";
        _regex = new Regex(regex, options);
    }
    public char Letter { get; private set; }
    public int Digit { get; private set; }
    public bool IsValid { get => Validate(this); }
    public Square(char letter, int number)
    {
        Letter = char.ToLower(letter);
        Digit = number;
    }
    public Square(Square other)
    {
        Letter = other.Letter;
        Digit = other.Digit;
    }
    public Square(string square)
    {
        var match = _regex.Match(square);
        if (match.Success)
        {
            Letter = char.ToLower(match.Groups["letter"].Value.First());

            if (int.TryParse(match.Groups["number"].Value, out int digit))
            {
                Digit = digit;
                return;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(square), "Not a valid chess square!");
    }
    public static bool IsLightSquare(Square square)
    {
        int letterInt = square.Letter - 'a' + 1;
        int digit = square.Digit;
        if (letterInt % 2 == 0)
            return digit % 2 != 0;
        else
            return digit % 2 == 0;
    }
    public void Transform((int letter, int digit) iterator)
    {
        Letter = (char)(Letter + iterator.letter);
        Digit += iterator.digit;
    }
    public static bool operator ==(Square first, Square second) => first.Letter == second.Letter && first.Digit == second.Digit;
    public static (int x, int y) operator -(Square first, Square second) => (first.Letter - second.Letter, first.Digit - second.Digit);
    public static bool Validate(Square square) => square.Digit >= 1 && square.Digit <= 8 && square.Letter >= 'a' && square.Letter <= 'h';
    public static bool operator !=(Square first, Square second) => !(first == second);
    public override bool Equals(object obj) => (obj is Square s) && this == s;
    public override int GetHashCode() => HashCode.Combine(Letter.GetHashCode(), Digit.GetHashCode());
    public override string ToString() => $"{Letter}{Digit}";
    public void Deconstruct(out char letter, out int digit)
    {
        letter = Letter;
        digit = Digit;
    }
}