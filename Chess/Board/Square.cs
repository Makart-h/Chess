using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Chess.Board
{
    internal struct Square
    {
        public static readonly int SquareWidth = 72;
        public static readonly int SquareHeight = SquareWidth;
        private static readonly Regex _regex;
        static Square()
        {
            RegexOptions options = RegexOptions.IgnoreCase;
            string regex = @"^(?'letter'[a-h]{1})(?'number'[1-8]{1})$";
            _regex = new Regex(regex, options);
        }
        public char Letter { get; private set; }
        public int Digit { get; private set; }
        public Square(char letter, int number)
        {
            Letter = char.ToUpper(letter);
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
                Letter = match.Groups["letter"].Value.First();
                Digit = int.Parse(match.Groups["number"].Value);
            }
            else
                throw new ArgumentOutOfRangeException("Not a valid chess square!");
        }
        public void Transform((int letter, int digit) iterator)
        {
            Letter = (char)(Letter + iterator.letter);
            Digit += iterator.digit;
        } 
        public static bool operator ==(Square first, Square second) => first.Letter == second.Letter && first.Digit == second.Digit;
        public static bool Validate(Square square) => square.Digit >= 1 && square.Digit <= 8 && square.Letter >= 'A' && square.Letter <= 'H';
        public static bool operator !=(Square first, Square second) => !(first == second);
        public override bool Equals(object obj) => (obj is Square s) && this == s;
        public override int GetHashCode() => HashCode.Combine(Letter.GetHashCode(), Digit.GetHashCode());
        public override string ToString() => $"{Letter}{Digit}";
    }
}