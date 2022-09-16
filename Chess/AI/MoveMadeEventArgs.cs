using System;
using Chess.Movement;
using Chess.Pieces;

namespace Chess.AI
{
    internal sealed class MoveMadeEventArgs : EventArgs
    {
        public readonly Controller Controller;
        public readonly Piece Piece;
        public readonly Move Move;

        public MoveMadeEventArgs(Controller controller, Piece piece, Move move)
        {
            Controller = controller;
            Piece = piece;
            Move = move;
        }
    }
}
