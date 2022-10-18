using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Pieces;

internal sealed class Pawn : Piece
{
    private bool _hasMoved;
    private bool _enPassant;
    private readonly int _promotionSquareNumber;
    private Square _leftEnPassant;
    private Square _rightEnPassant;
    private Square _oneUp;
    private readonly Square _doubleMove;
    public static EventHandler Promotion;
    public bool EnPassant { get { return _enPassant; } set { _enPassant = value; } }
    public Pawn(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Pawn, isRaw)
    {
        _enPassant = false;
        _moveset = Movesets.Pawn;
        _promotionSquareNumber = team == Team.White ? 8 : 1;
        Value = team == Team.White ? 1 : -1;
        if (_promotionSquareNumber == 8 && square.Digit != 2 || _promotionSquareNumber == 1 && square.Digit != 7)
            _hasMoved = true;

        _leftEnPassant = new Square(Square, (-1, 0));
        _rightEnPassant = new Square(Square, (1, 0));
        _oneUp = new Square(Square, (0, Value));
        _doubleMove = new Square(Square, (0, Value * 2));
    }
    public Pawn(Pawn other, bool isRaw = false) : base(other, isRaw)
    {
        _enPassant = other._enPassant;
        _hasMoved = other._hasMoved;
        _promotionSquareNumber = other._promotionSquareNumber;
        _leftEnPassant = other._leftEnPassant;
        _rightEnPassant = other._rightEnPassant;
        _oneUp = other._oneUp;
        _doubleMove = other._doubleMove;
    }
    public override void Update()
    {
        if (_enPassant)
            _enPassant = false;
        base.Update();
    }
    public override void CheckPossibleMoves()
    {
        _moves = new(2);
        Team teamOneUp = Owner.GetTeamOnSquare(_oneUp);  
        // Regular move.
        if (teamOneUp == Team.Empty)
        {
            // Double move that's available at the start position.
            if (!_hasMoved)
                CheckDoubleMove();
            Move moveToAdd = new(Square, _oneUp, 'm');
            _moves.Add(moveToAdd);
        }
        // En passant to the right.
        CheckEnPassant(_rightEnPassant);
        // En passant to the left.
        CheckEnPassant(_leftEnPassant);
        // Add captures and filter through king.
        _moves.AddRange(MovementManager.GenerateEveryMove(Square, _moveset, Owner, friendlyFire: IsRaw));
        _moves = Owner.GetKing(_team).FilterMovesThroughThreats(_moves);
    }
    private void CheckDoubleMove()
    {
        Team teamOnTheSecondSquare = Owner.GetTeamOnSquare(_doubleMove);
        if (teamOnTheSecondSquare == Team.Empty)
            _moves.Add(new Move(Square, _doubleMove, 'm'));
    }
    private void CheckEnPassant(Square square)
    {
        if (Square.Validate(square))
        {
            Team teamOnTheSquare = Owner.GetTeamOnSquare(square);
            if (teamOnTheSquare != _team && teamOnTheSquare != Team.Empty)
            {
                Piece piece = Owner.GetPiece(square);
                if (piece is Pawn p && p._enPassant == true)
                    _moves.Add(new(Square, new Square(square, (0, Value)), 'p'));
            }
        }
    }
    public override void MovePiece(in Move move)
    {
        if (move.Latter == _doubleMove)
            _enPassant = true;
        Square = move.Latter;
        _hasMoved = true;
        UpdateSquares(move.Former, move.Latter);

        if(!IsRaw)
            OnPieceMoved(new PieceMovedEventArgs(this, move));

        if (Square.Digit == _promotionSquareNumber)
        {
            if (IsRaw)
                Owner.OnPromotion(this);
            else
                Promotion?.Invoke(this, EventArgs.Empty);
        }       
    }
    private void UpdateSquares(Square oldSquare, Square newSquare)
    {
        var vector = newSquare - oldSquare;
        _oneUp.Transform(vector);
        _leftEnPassant.Transform(vector);
        _rightEnPassant.Transform(vector);
    }
}
