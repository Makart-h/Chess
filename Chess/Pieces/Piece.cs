using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Graphics;
using Chess.Board;
using Chess.AI;

namespace Chess.Pieces
{
    abstract class Piece : DrawableObject, IComparable<Piece>
    {
        protected readonly Team team;
        public Team Team { get => team; }
        protected Square square;
        protected bool isSelected;
        protected MoveSets moveSet;
        public IPieceOwner Owner { get; set; }

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
        public bool IsRawPiece { get; protected set; }
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
        public List<Move> Moves { get => moves; set => moves = value; }
        protected List<Move> CopyMoves()
        {
            List<Move> copy = new List<Move>(Moves.Count);
            foreach (var move in moves)
            {
                copy.Add(new Move(move));
            }
            return copy;
        }
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
                    if (Owner.GetKing(team).CheckMoveAgainstThreats(this, move))
                        moves.Add(move);
                }
            }
        }
        public abstract void MovePiece(Move move);
        public virtual int Update()
        {
            moves.Clear();
            CheckPossibleMoves();
            return moves.Count;
        }
        protected virtual void OnPieceMoved(PieceMovedEventArgs e)
        {
            if (IsRawPiece)
                return;

            PieceMoved?.Invoke(this, e);
        }
        protected virtual void OnPieceSelected(PieceSelectedEventArgs e)
        {
            if (IsRawPiece)
                return;

            PieceSelected?.Invoke(this, e);
        }
        protected virtual void OnPieceDeselected(EventArgs e)
        {
            if (IsRawPiece)
                return;

            PieceDeselected?.Invoke(this, e);
        }
        public int CompareTo(Piece other) => Math.Abs(Value).CompareTo(Math.Abs(other.Value));
    }
}
