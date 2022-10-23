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
    public List<((int x, int y) direction, Square square)> InvolvedSquares { get => _involvedSquares; }
    private readonly List<((int x, int y) direction, Square square)> _involvedSquares;

    public SquareControl(in Square controlledSquare)
    {
        _involvedSquares = new(4);
        TeamInControl = Team.Empty;
        ControlledSquare = controlledSquare;
    }
    public void AddSquare(in Square squareToAdd, Team teamOnSquare)
    {
        (int x, int y) direction = ControlledSquare - squareToAdd;
        if (direction.x == 0 || direction.y == 0 || Math.Abs(direction.x) == Math.Abs(direction.y))
            direction = (Math.Sign(direction.x), Math.Sign(direction.y));
        _involvedSquares.Add((direction, squareToAdd));
        ControlValue += teamOnSquare == Team.White ? 1 : -1;
    }
    public Square? GetInvolvedSquare((int x, int y) direction)
    {
        for(int i = 0; i < _involvedSquares.Count; i++)
        {
            if (_involvedSquares[i].direction == direction)
                return _involvedSquares[i].square;
        }
        return null;
    }
}
