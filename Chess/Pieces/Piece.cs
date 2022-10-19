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
    public Movesets Moveset { get => _moveset; }
    public PieceType Type { get; init; }
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
    protected List<Move> _moves;

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
    public List<Move> Moves { get => _moves; set => _moves = value; }
    public Move? GetAMove(Square destination)
    {
        foreach (Move move in _moves)
        {
            if (move.Latter == destination)
                return move;
        }
        return null;
    }
    public virtual void CheckPossibleMoves()
    {
        _moves = MoveGenerator.GenerateEveryMove(Square, _moveset, Owner, friendlyFire: IsRaw);
        _moves = Owner.GetKing(_team).FilterMovesThroughThreats(_moves);
    }
    public virtual void MovePiece(in Move move)
    {
        Square = move.Latter;
        if(!IsRaw)
            OnPieceMoved(new PieceMovedEventArgs(this, move));       
    }
    public virtual void Update()
    {
        CheckPossibleMoves();
    }
    protected virtual void OnPieceMoved(PieceMovedEventArgs e)
    {
        PieceMoved?.Invoke(this, e);
    }
    protected virtual void OnPieceSelected(PieceEventArgs e)
    {
        PieceSelected?.Invoke(this, e);
    }
    protected virtual void OnPieceDeselected(PieceEventArgs e)
    {
        PieceDeselected?.Invoke(this, e);
    }
    public int CompareTo(Piece other)
    {
        if (Value == 0)
            return 1;
        else if (other.Value == 0)
            return -1;
        else
            return Math.Abs(Value).CompareTo(Math.Abs(other.Value));
    }
}
