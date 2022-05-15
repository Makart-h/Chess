using System;
using System.Collections.Generic;
using System.Text;
using Chess.Movement;
using Chess.Pieces;

namespace Chess.AI
{
    internal class MoveChosenEventArgs : EventArgs
    {
        public readonly Controller Controller;
        public readonly Piece Piece;
        public readonly Move Move;

        public MoveChosenEventArgs(Controller controller, Piece piece, Move move)
        {
            Controller = controller;
            Piece = piece;
            Move = move;
        }
    }
}
