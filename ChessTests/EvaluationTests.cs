using Xunit;
using Chess.Positions;

namespace ChessTests
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData(0.25, 3)]
        [InlineData(1000, 1)]
        [InlineData(-1000, 0)]
        [InlineData(-5.74, -234)]
        internal void CompareTo_ShouldBeZeroForItself(double value, int depth)
        {
            Evaluation evaluation = new() { Value = value, Depth = depth};
            int actual = evaluation.CompareTo(evaluation, true);
            Assert.Equal(0, actual);
        }
        [Theory]
        [InlineData(2.3, 4, 2.3, 4, 0)]
        [InlineData(2.3, 4, 2.3, 6, -1)]
        [InlineData(6, 4, 2, 4, 1)]
        [InlineData(-1000, 2, -1000, 8, -1)]
        [InlineData(1000, 2, 1000, 8, 1)]
        internal void CompareTo_ShouldWorkWithStandardComparison(double firstValue, int firstDepth, double secondValue, int secondDepth, int expected)
        {
            Evaluation first = new() { Value = firstValue, Depth = firstDepth };
            Evaluation second = new() { Value = secondValue, Depth = secondDepth };
            int actual = first.CompareTo(second, true);
            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData(2.3, 4, 2.3, 4, 0)]
        [InlineData(2.3, 4, 2.3, 6, -1)]
        [InlineData(6, 4, 2, 4, -1)]
        [InlineData(-1000, 2, -1000, 8, 1)]
        [InlineData(1000, 2, 1000, 8, -1)]
        internal void CompareTo_ShouldWorkWithAlternativeComparison(double firstValue, int firstDepth, double secondValue, int secondDepth, int expected)
        {
            Evaluation first = new() { Value = firstValue, Depth = firstDepth };
            Evaluation second = new() { Value = secondValue, Depth = secondDepth };
            int actual = first.CompareTo(second, false);
            Assert.Equal(expected, actual);
        }
        [Theory]
        [MemberData(nameof(EvaluationsWithStandardComparison))]
        internal void Max_ShouldWorkWithStandardComparison(Evaluation expected, params Evaluation[] evaluations)
        {
            Evaluation max = Evaluation.Max(evaluations, true);
            Assert.Equal(expected, max);
        }
        [Theory]
        [MemberData(nameof(EvaluationsWithAlternativeComparison))]
        internal void Max_ShouldWorkWithAlternativeComparison(Evaluation expected, params Evaluation[] evaluations)
        {
            Evaluation max = Evaluation.Max(evaluations, false);
            Assert.Equal(expected, max);
        }
        public static IEnumerable<object[]> EvaluationsWithStandardComparison()
        {
            yield return new object[] {
            new Evaluation() { Value = 1000, Depth = 2 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 3.2, Depth = 2 },
                new() { Value = 1000, Depth = 6 },
                new() { Value = 1000, Depth = 2 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
            yield return new object[] {
            new Evaluation() { Value = 7.34, Depth = 11 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 7.12, Depth = 2 },
                new() { Value = 7.34, Depth = 6 },
                new() { Value = 7.34, Depth = 11 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
            yield return new object[] {
            new Evaluation() { Value = 3.2, Depth = 2 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 3.2, Depth = 2 },
                new() { Value = -1000, Depth = 1 },
                new() { Value = -1000, Depth = 5 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
        }
        public static IEnumerable<object[]> EvaluationsWithAlternativeComparison()
        {
            yield return new object[] {
            new Evaluation() { Value = -64, Depth = 5 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 3.2, Depth = 2 },
                new() { Value = 1000, Depth = 6 },
                new() { Value = 1000, Depth = 2 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
            yield return new object[] {
            new Evaluation() { Value = -64, Depth = 5 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 7.12, Depth = 2 },
                new() { Value = 7.34, Depth = 6 },
                new() { Value = 7.34, Depth = 11 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
            yield return new object[] {
            new Evaluation() { Value = -1000, Depth = 1 },
            new Evaluation[] { new() { Value = 3, Depth = 2 },
                new() { Value = 3.2, Depth = 2 },
                new() { Value = -1000, Depth = 1 },
                new() { Value = -1000, Depth = 5 },
                new() { Value = -5, Depth = 11 },
                new() { Value = -64, Depth = 5 } }
            };
        }
    }
}
