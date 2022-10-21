using Xunit;
using Chess.Movement;
using Chess.Board;

namespace ChessTests
{
    public class MoveGeneratorTests
    {
        [Theory]
        [InlineData("a1", -1, 0, 0)]
        [InlineData("b2", -1, -1, 1)]
        [InlineData("b2", -2, -1, 0)]
        [InlineData("c6", 1, 1, 2)]
        [InlineData("f6", 0, 0, 0)]
        [InlineData("c4", 2, 0, 2)]
        public void GetNumberOfPossibleValidSquareIterations_ShouldWork(string square, int xIt, int yIt, int expected)
        {
            Square initialSquare = new(square);
            int actual = MoveGenerator.GetNumberOfPossibleValidSquareIterations(initialSquare, (xIt, yIt));
            Assert.Equal(expected, actual);
        }
    }
}
