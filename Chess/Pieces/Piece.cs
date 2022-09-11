using System;
using System.Collections.Generic;
using Chess.Board;
using Chess.Graphics;
using Chess.Movement;

namespace Chess.Pieces
{
    abstract class Piece : DrawableObject, IComparable<Piece>
    {
        protected readonly Team _team;
        protected bool _isSelected;
        protected MoveSets _moveSet;
        public Team Team { get => _team; }
        public IPieceOwner Owner { get; set; }
        public static event EventHandler<PieceMovedEventArgs> PieceMoved;
        public static event EventHandler<PieceEventArgs> PieceSelected;
        public static event EventHandler PieceDeselected;
        public int Value { get; protected set; }
        public MoveSets MoveSet { get => _moveSet; }
        public bool IsSelected { 
            get => _isSelected;
            set {
                _isSelected = value;
                if (_isSelected)
                    OnPieceSelected(new PieceEventArgs(this));
                else
                    OnPieceDeselected(EventArgs.Empty);
                _color.A = _isSelected ? (byte)100 : (byte)255;
            }
        }
        public bool IsRawPiece { get; protected set; }
        public Square Square { get; set; }
        protected List<Move> moves;

        protected Piece(Team team, Square square)
        {
            _team = team;
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
        public Move GetAMove(Square targetedSquare)
        {
            foreach (Move move in moves)
            {
                if (move.Latter == targetedSquare)
                    return move;
            }
            return null;
        }
        public virtual void CheckPossibleMoves()
        {
            (MoveSets sets, Move[] moves)[] groupedMoves = Move.GenerateEveryMove(Square, _moveSet, Owner);
            foreach (var group in groupedMoves)
            {
                foreach (Move move in group.moves)
                {
                    if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, move))
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
