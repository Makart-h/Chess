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
        private (char letter, int digit) number;
        public (char letter, int digit) Number
        {
            get => number;

            set
            {
                char letter = char.ToUpper(value.letter);
                if (char.IsLetter(letter))
                {
                    if (letter >= 'A' && letter <= 'H' && value.digit >= 1 && value.digit <= 8)
                    {
                        number = (letter, value.digit);
                        return;
                    }
                }
                throw new ArgumentOutOfRangeException();
            }
        }
        public Square(char letter, int number)
        {
            this.number.letter = char.ToUpper(letter);
            this.number.digit = number;
        }
        public Square(Square other)
        {
            number.letter = other.number.letter;
            number.digit = other.number.digit;
        }
        public Square(string square)
        {
            var match = _regex.Match(square);
            if (match.Success)
            {
                number.letter = match.Groups["letter"].Value.First();
                number.digit = int.Parse(match.Groups["number"].Value);
            }
            else
                throw new ArgumentOutOfRangeException("Not a valid chess square!");
        }
        public void Transform((int letter, int digit) iterator)
        {
            number.letter = (char)(number.letter + iterator.letter);
            number.digit += iterator.digit;
        } 
        public static bool operator ==(Square first, Square second) => first.number.letter == second.number.letter && first.number.digit == second.number.digit;
        public static bool Validate(Square square) => square.Number.digit >= 1 && square.Number.digit <= 8 && square.Number.letter >= 'A' && square.Number.letter <= 'H';
        public static bool operator !=(Square first, Square second) => !(first == second);
        public override bool Equals(object obj) => (obj is Square s) && this == s;
        public override int GetHashCode() => HashCode.Combine(number.letter.GetHashCode(), number.digit.GetHashCode());
        public override string ToString() => $"{number.letter}{number.digit}";
    }
}