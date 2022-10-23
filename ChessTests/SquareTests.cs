using Xunit;
using Chess.Board;

namespace ChessTests;

public class SquareTests
{
    [Theory]
    [InlineData('b', 1)]
    [InlineData('e', 2)]
    [InlineData('d', 3)]
    [InlineData('g', 4)]
    [InlineData('b', 5)]
    [InlineData('c', 6)]
    [InlineData('h', 7)]
    [InlineData('a', 8)]
    [InlineData('d', 1)]
    public void IsLightSquare_ShouldBeTrueForLightSquares(char letter, int digit)
    {
        Square square = new(letter, digit);
        bool actual = Square.IsLightSquare(square);
        Assert.True(actual);
    }
    [Theory]
    [InlineData('a', 1)]
    [InlineData('h', 2)]
    [InlineData('g', 3)]
    [InlineData('d', 4)]
    [InlineData('a', 5)]
    [InlineData('b', 6)]
    [InlineData('g', 7)]
    [InlineData('f', 8)]
    [InlineData('d', 8)]
    public void IsLightSquare_ShouldBeFalseForDarkSquares(char letter, int digit)
    {
        Square square = new(letter, digit);
        bool actual = Square.IsLightSquare(square);
        Assert.False(actual);
    }
    [Theory]
    [InlineData(' ', -3)]
    [InlineData('t', 1)]
    [InlineData('a', 9)]
    [InlineData('h', 0)]
    [InlineData('#', 342)]
    public void IsLightSquare_ShouldThrowForInvalidSquares(char letter, int digit)
    {
        Square square = new(letter, digit);
        Assert.Throws<ArgumentException>(() => Square.IsLightSquare(square));
    }
    [Theory]
    [InlineData("@2")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("x32")]
    [InlineData("a3d4")]
    [InlineData(" a2")]
    [InlineData("d4 ")]
    public void Square_ShouldThrowForInvalidValues(string square)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Square(square));
    }
    [Fact]
    public void Transform_ShouldNotChangeWithZeroIterator()
    {
        Square firstSquare = new("a4");
        Square secondSquare = firstSquare;
        firstSquare = firstSquare.Transform((0, 0));
        Assert.Equal(firstSquare, secondSquare);
    }
    [Theory]
    [InlineData("a4", 1, 1, "b5")]
    [InlineData("a4", 1, 0, "b4")]
    [InlineData("c6", 3, -1, "f5")]
    [InlineData("c8", -2, -2, "a6")]
    public void Transform_ShouldWork(string initialSquare, int xIt, int yIt, string expectedSquare)
    {
        Square initial = new(initialSquare);
        Square expected = new(expectedSquare);
        initial = initial.Transform((xIt, yIt));
        Assert.Equal(expected, initial);
    }
    [Theory]
    [InlineData("c4")]
    [InlineData("b8")]
    [InlineData("a1")]
    [InlineData("e5")]
    public void Validate_ShouldBeTrueForValidSquares(string squareString)
    {
        Square square = new(squareString);
        bool actual = Square.Validate(square);
        Assert.True(actual);
    }
    [Theory]
    [InlineData(' ', 3)]
    [InlineData('a', 15)]
    [InlineData('#', -2)]
    [InlineData('i', 2)]
    public void Validate_ShouldBeFalseForInvalidSquares(char letter, int digit)
    {
        Square square = new(letter, digit);
        bool actual = Square.Validate(square);
        Assert.False(actual);
    }
    [Theory]
    [InlineData('a', 1, 0)]
    [InlineData('h', 8, 63)]
    [InlineData('d', 4, 27)]
    public void Index_ShouldBeCorrectlyCalculated(char letter, int digit, int expected)
    {
        Square square = new(letter, digit);
        int actual = square.Index;
        Assert.Equal(expected, actual);
    }
}
