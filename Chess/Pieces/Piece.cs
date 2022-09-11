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
        protected bool isSelected;
        protected MoveSets moveSet;
        public IPieceOwner Owner { get; set; }

        public static event EventHandler<PieceMovedEventArgs> PieceMoved;
        public static event EventHandler<PieceEventArgs> PieceSelected;
        public static event EventHandler PieceDeselected;
        public int Value { get; protected set; }
        public MoveSets MoveSet { get => moveSet; }
        public bool IsSelected { 
            get => isSelected;
            set {
                isSelected = value;
                if (isSelected)
                    OnPieceSelected(new PieceEventArgs(this));
                else
                    OnPieceDeselected(EventArgs.Empty);
                _color.A = isSelected ? (byte)100 : (byte)255;
            }
        }
        public bool IsRawPiece { get; protected set; }
        public Square Square { get; set; }
        protected List<Move> moves;

        protected Piece(Team team, Square square)
        {
            this.team = team;
            Square = square;
            Position = Chessboard.Instance.ToCordsFromSquare(square);
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
        public virtual void CheckPossibleMoves()
        {
            (MoveSets sets, Move[] moves)[] groupedMoves = Move.GenerateEveryMove(Square, moveSet, Owner);
            foreach (var group in groupedMoves)
            {
                foreach (var move in group.moves)
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
        protected virtual void OnPieceSelected(PieceEventArgs e)
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
