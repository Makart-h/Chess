using Chess.Board;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Positions.Evaluators;

internal class SquareControl
{
    public Square ControlledSquare { get; init; }
    public int ControlValue { get; private set; }
    public Team TeamInControl { get; set; }
    public Dictionary<(int x, int y), Square> InvolvedSquares { get; init; }

    public SquareControl(Square controlledSquare)
    {
        InvolvedSquares = new();
        TeamInControl = Team.Empty;
        ControlledSquare = controlledSquare;
    }
    public void AddSquare(Square squareToAdd, Team teamOnSquare)
    {
        (int x, int y) direction = ControlledSquare - squareToAdd;
        if (direction.x == 0 || direction.y == 0 || Math.Abs(direction.x) == Math.Abs(direction.y))
            direction = (Math.Sign(direction.x), Math.Sign(direction.y));
        InvolvedSquares[direction] = squareToAdd;
        ControlValue += teamOnSquare == Team.White ? 1 : -1;
    }
    public bool TryGetInvolvedSquare((int x, int y) direction, out Square involvedSquare)
    {
        if (InvolvedSquares.ContainsKey(direction))
        {
            involvedSquare = InvolvedSquares[direction];
            return true;
        }
        else
        {
            involvedSquare = new();
            return false;
        }
    }
}
