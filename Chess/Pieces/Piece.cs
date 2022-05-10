using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Graphics;
using Chess.Board;

namespace Chess.Pieces
{
    abstract class Piece : DrawableObject
    {
        protected readonly Team team;
        public Team Team { get => team; }
        protected Square square;
        protected bool isSelected;
        protected MoveSets moveSet;

        public static event EventHandler<PieceMovedEventArgs> PieceMoved;
        public static event EventHandler<PieceSelectedEventArgs> PieceSelected;
        public static event EventHandler PieceDeselected;
        public int Value { get; protected set; }
        public MoveSets MoveSet { get => moveSet; }
        public bool IsSelected { 
            get => isSelected;
            set {
                isSelected = value;
                if (isSelected)
                    OnPieceSelected(new PieceSelectedEventArgs(this));
                else
                    OnPieceDeselected(EventArgs.Empty);
                color.A = isSelected ? (byte)100 : (byte)255;
            }
        }
        public override Vector2 Position { get => Chessboard.ToCordsFromSquare(square); }
        public Square Square { get => square; set => square = value; }
        protected List<Move> moves;

        //methods
        protected Piece(Team team, Square square)
        {
            this.team = team;
            this.square = square;
            moves = new List<Move>();
        }
        public List<Move> Moves { get => moves; }
        public Move GetAMove(Square move)
        {
            foreach (var item in moves)
            {
                if (item.Latter == move)
                    return item;
            }
            return null;
        }
        public bool CanMoveToSquare(Square square)
        {
            foreach(var move in moves)
            {
                if (move.Latter == square)
                    return true;
            }
            return false;
        }
        public virtual void CheckPossibleMoves()
        {
            List<(MoveSets, List<Move>)> groupedMoves = Move.GenerateEveryMove(this, moveSet);
            foreach (var group in groupedMoves)
            {
                foreach (var move in group.Item2)
                {
                    if (Chessboard.Instance.GetKing(team).CheckMoveAgainstThreats(this, move))
                        moves.Add(move);
                }
            }
        }
        public abstract void MovePiece(Move move);
        public abstract void Update();

        protected virtual void OnPieceMoved(PieceMovedEventArgs e)
        {
            PieceMoved?.Invoke(this, e);
        }

        protected virtual void OnPieceSelected(PieceSelectedEventArgs e)
        {
            PieceSelected?.Invoke(this, e);
        }
        protected virtual void OnPieceDeselected(EventArgs e)
        {
            PieceDeselected?.Invoke(this, e);
        }
    }
}
