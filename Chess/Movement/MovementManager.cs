using Chess.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Pieces;

namespace Chess.Movement
{
    internal static class MovementManager
    {
        public static List<Initiator> initiators;
        public static Stack<Initiator> toRemove;
        public static float MovementVelocity { get; private set; }
        static MovementManager()
        {
            initiators = new List<Initiator>();
            toRemove = new Stack<Initiator>();
            MovementVelocity = 0.4f;
            Piece.PieceMoved += OnPieceMoved;
        }

        public static void OnPieceMoved(object sender, PieceMovedEventArgs args)
        {
            Vector2 newPosition = Chessboard.Instance.ToCordsFromSquare(args.Move.Latter);
            if (args.Piece.Owner is HumanController && args.Move.Description != 'c')
            {            
                args.Piece.MoveObject(newPosition-args.Piece.Position);
            }
            else
            {
                initiators.Add(new Initiator(newPosition, args.Piece, OnDestinationReached));
            }
        }
        public static void OnDestinationReached(object sender, EventArgs args)
        {
            if(sender is Initiator i)
            {
                i.Dispose();
                toRemove.Push(i);
            }
        }
        public static void Update(GameTime gameTime)
        {
            foreach(var initiator in initiators)
            {
                initiator.Update(gameTime);
            }
            while(toRemove.TryPop(out Initiator i))
            {
                initiators.Remove(i);
            }
        }
        public static Vector2 RecalculateVector(Vector2 vector)
        {
            float width = Square.SquareWidth * (Chessboard.NumberOfSquares-1);
            float height = Square.SquareHeight * (Chessboard.NumberOfSquares-1);
            return new Vector2(width - vector.X, height - vector.Y);
        }
    }
}
