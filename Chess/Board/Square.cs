using System;

namespace Chess.Board
{
    internal struct Square
    {
        public static readonly int SquareWidth = 72; //???
        public static readonly int SquareHeight = SquareWidth;
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
        public static bool operator ==(Square first, Square second)
        {
            return first.number.letter == second.number.letter && first.number.digit == second.number.digit;
        }
        public static bool operator !=(Square first, Square second) => !(first == second);
        public override bool Equals(object obj) => (obj is Square s) && this == s;
        public override int GetHashCode() => HashCode.Combine(number.letter.GetHashCode(), number.digit.GetHashCode());
        public override string ToString() => $"{number.letter}{number.digit}";
    }
}