using Chess.Board;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Model = Chess.Graphics.Model;

namespace Chess.Pieces;

internal abstract class Piece : DrawableObject, IComparable<Piece>
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
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (_isSelected)
                OnPieceSelected(new PieceEventArgs(this));
            else
                OnPieceDeselected(EventArgs.Empty);
            _color.A = _isSelected ? (byte)10 : (byte)255;
        }
    }
    public bool IsRaw { get; protected set; }
    public Square Square { get; set; }
    protected List<Move> moves;

    protected Piece(Team team, Square square, PieceType type, bool isRaw)
        : base(null, new Rectangle((int)Chessboard.Instance.ToCordsFromSquare(square).X, (int)Chessboard.Instance.ToCordsFromSquare(square).Y, Chessboard.Instance.SquareSideLength, Chessboard.Instance.SquareSideLength))
    {
        IsRaw = isRaw;
        if (!IsRaw)
        {
            Texture2D rawTexture = PieceFactory.PiecesRawTexture;
            int texturePosX = PieceFactory.PieceTextureWidth * (int)type + (rawTexture.Width / 2 * ((byte)team & 1));
            Model = new Model(rawTexture, texturePosX, 0, PieceFactory.PieceTextureWidth, PieceFactory.PieceTextureWidth);
        }      
        _team = team;
        Square = square;
        moves = new();
    }
    protected Piece(Piece other, bool isRaw) : base(null, Rectangle.Empty)
    {
        IsRaw = isRaw;
        Model = IsRaw ? null : new Model(other.Model);
        _team = other._team;
        Square = other.Square;
        moves = other.CreateCopyOfMoves();
        _moveSet = other._moveSet;
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
