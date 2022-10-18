using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Pieces;

internal abstract class Piece : IComparable<Piece>
{
    protected readonly Team _team;
    protected bool _isSelected;
    protected Movesets _moveset;
    public Team Team { get => _team; }
    public IPieceOwner Owner { get; set; }
    public static event EventHandler<PieceMovedEventArgs> PieceMoved;
    public static event EventHandler<PieceEventArgs> PieceSelected;
    public static event EventHandler<PieceEventArgs> PieceDeselected;
    public int Value { get; protected set; }
    public MoveSets MoveSet { get => _moveSet; }
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (!IsRaw)
            {
            if (_isSelected)
                OnPieceSelected(new PieceEventArgs(this));
            else
                    OnPieceDeselected(new PieceEventArgs(this));
        }
    }
    }
    public bool IsRaw { get; protected set; }
    public Square Square { get; set; }
    protected List<Move> moves;

    protected Piece(Team team, Square square, PieceType type, bool isRaw)
    {
        IsRaw = isRaw;
        _team = team;
        Square = square;
        Type = type;
        _moves = new();
    }
    protected Piece(Piece other, bool isRaw)
    {
        IsRaw = isRaw;
        _team = other._team;
        Square = other.Square;
        _moves = new();
        _moveset = other._moveset;
        Value = other.Value;
        Owner = other.Owner;
    }
    public List<Move> Moves { get => moves; set => moves = value; }
    protected List<Move> CreateCopyOfMoves()
    {
        List<Move> copy = new(Moves.Count);
        foreach (var move in moves)
        {
            copy.Add(new Move(move));
        }
        return copy;
    }
    public Move GetAMove(Square destination)
    {
        foreach (Move move in moves)
        {
            if (move.Latter == destination)
                return move;
        }
        return null;
    }
    public virtual void CheckPossibleMoves()
    {
        (MoveSets sets, Move[] moves)[] groupedMoves = MovementManager.GenerateEveryMove(Square, _moveSet, Owner);
        foreach (var group in groupedMoves)
        {
            foreach (Move move in group.moves)
            {
                if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, move))
                    moves.Add(move);
            }
        }
    }
    public virtual void MovePiece(Move move)
    {
        OnPieceMoved(new PieceMovedEventArgs(this, move));
        Square = move.Latter;
    }
    public virtual int Update()
    {
        moves.Clear();
        CheckPossibleMoves();
        return moves.Count;
    }
    protected virtual void OnPieceMoved(PieceMovedEventArgs e)
    {
        if (IsRaw)
            return;

        PieceMoved?.Invoke(this, e);
    }
    protected virtual void OnPieceSelected(PieceEventArgs e)
    {
        if (IsRaw)
            return;

        PieceSelected?.Invoke(this, e);
    }
    protected virtual void OnPieceDeselected(EventArgs e)
    {
        if (IsRaw)
            return;

        PieceDeselected?.Invoke(this, e);
    }
    public int CompareTo(Piece other) => Math.Abs(Value).CompareTo(Math.Abs(other.Value));
}
