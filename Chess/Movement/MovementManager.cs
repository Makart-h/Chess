using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chess.AI;
using Chess.Board;
using Chess.Pieces;

namespace Chess.Movement
{
    internal static class MovementManager
    {
        public static List<Initiator> _initiators;
        public static Stack<Initiator> _toRemove;
        public static EventHandler MovementConcluded;
        public static float MovementVelocity { get; private set; }
        static MovementManager()
        {
            _initiators = new List<Initiator>();
            _toRemove = new Stack<Initiator>();
            MovementVelocity = 0.4f;
            Piece.PieceMoved += OnPieceMoved;
        }
        private static void OnPieceMoved(object sender, PieceMovedEventArgs args)
        {
            Vector2 newPosition = Chessboard.Instance.ToCordsFromSquare(args.Move.Latter);
            if (args.Piece.Owner is HumanController && args.Move.Description != 'c')
            {            
                args.Piece.MoveObject(newPosition-args.Piece.Position);
            }
            else
            {
                _initiators.Add(new Initiator(newPosition, args.Piece, OnDestinationReached));
            }
        }
        private static void OnDestinationReached(object sender, EventArgs args)
        {
            if(sender is Initiator i)
            {
                i.Dispose();
                _toRemove.Push(i);
            }
        }
        public static void Update(GameTime gameTime)
        {
            bool initiatorRemovedOnThisUpdate = false;
            foreach(Initiator initiator in _initiators)
            {
                initiator.Update(gameTime);
            }
            while(_toRemove.TryPop(out Initiator i))
            {
                _initiators.Remove(i);
                initiatorRemovedOnThisUpdate = true;
            }
            if (_initiators.Count == 0 && initiatorRemovedOnThisUpdate)
                OnMovementConcluded(EventArgs.Empty);
        }
        public static Vector2 RecalculateVector(Vector2 vector)
        {
            float width = Square.SquareWidth * (Chessboard.NumberOfSquares-1);
            float height = Square.SquareHeight * (Chessboard.NumberOfSquares-1);
            return new Vector2(width - vector.X, height - vector.Y);
        }
        private static void OnMovementConcluded(EventArgs e) => MovementConcluded?.Invoke(null, e);
    }
}
