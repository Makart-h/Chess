using Chess.Movement;
using Chess.Pieces;
using System;

namespace Chess.AI;

internal sealed class MoveMadeEventArgs : EventArgs
{
    public Controller Controller { get; init; }
    public Piece Piece { get; init; }
    public Move Move { get; init; }

    public MoveMadeEventArgs(Controller controller, Piece piece, Move move)
    {
        Controller = controller;
        Piece = piece;
        Move = move;
    }
}
