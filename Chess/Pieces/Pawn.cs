using Chess.Board;
using Chess.Movement;
using Chess.Pieces.Info;
using System;

namespace Chess.Pieces;

internal sealed class Pawn : Piece
{
    private bool _hasMoved;
    private bool _enPassant;
    private readonly int _promotionSquareNumber;
    public static EventHandler Promotion;
    public bool EnPassant { get { return _enPassant; } set { _enPassant = value; } }
    public Pawn(Team team, Square square, bool isRaw = false) : base(team, square, PieceType.Pawn, isRaw)
    {
        _enPassant = false;
        _moveSet = MoveSets.Pawn;
        _promotionSquareNumber = team == Team.White ? 8 : 1;
        Value = team == Team.White ? 1 : -1;
        if (_promotionSquareNumber == 8 && square.Digit != 2 || _promotionSquareNumber == 1 && square.Digit != 7)
            _hasMoved = true;
    }
    public Pawn(Pawn other, bool isRaw = false) : base(other, isRaw)
    {
        _enPassant = other._enPassant;
        _hasMoved = other._hasMoved;
        _promotionSquareNumber = other._promotionSquareNumber;
    }
    public override int Update()
    {
        if (_enPassant)
            _enPassant = false;
        return base.Update();
    }
    public override void CheckPossibleMoves()
    {
        Team teamOnTheSquare;
        Square square;
        int direction = _team == Team.White ? 1 : -1;

        // Double move that's available at the start position.
        CheckDoubleMove(direction);

        // Regular move.
        square = new Square(Square.Letter, Square.Digit + (1 * direction));
        teamOnTheSquare = Owner.GetTeamOnSquare(square);
        if (teamOnTheSquare == Team.Empty)
        {
            Move moveToAdd = new Move(Square, square, 'm');
            if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                moves.Add(moveToAdd);
        }

        // Captures.
        base.CheckPossibleMoves();

        // En passant to the right.
        CheckEnPassant(new Square((char)(Square.Letter + 1), Square.Digit), direction);
        // En passant to the left.
        CheckEnPassant(new Square((char)(Square.Letter - 1), Square.Digit), direction);
    }
    private void CheckDoubleMove(int direction)
    {
        if (!_hasMoved)
        {
            Square square = new Square(Square.Letter, Square.Digit + 1 * direction);
            if (Square.Validate(Square))
            {
                Team teamOnTheSquare = Owner.GetTeamOnSquare(square);
                if (teamOnTheSquare == Team.Empty)
                {
                    square = new Square(Square.Letter, Square.Digit + 2 * direction);
                    if (Square.Validate(square))
                    {
                        teamOnTheSquare = Owner.GetTeamOnSquare(square);
                        if (teamOnTheSquare == Team.Empty)
                        {
                            Move moveToAdd = new Move(Square, square, 'm');
                            if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                                moves.Add(moveToAdd);
                        }
                    }
                }
            }
        }
    }
    private void CheckEnPassant(Square square, int direction)
    {
        if (Square.Validate(square))
        {
            Team teamOnTheSquare = Owner.GetTeamOnSquare(square);
            if (teamOnTheSquare != _team && teamOnTheSquare != Team.Empty)
            {
                if (Owner.TryGetPiece(square, out Piece piece) && piece is Pawn p && p._enPassant == true)
                {
                    Move moveToAdd = new Move(Square, new Square(square.Letter, Square.Digit + (1 * direction)), 'p');
                    if (Owner.GetKing(_team).CheckMoveAgainstThreats(this, moveToAdd))
                        moves.Add(moveToAdd);
                }
            }
        }
    }
    public override void MovePiece(Move move)
    {
        if (Math.Abs(Square.Digit - move.Latter.Digit) == 2)
            _enPassant = true;
        Square = move.Latter;
        _hasMoved = true;

        OnPieceMoved(new PieceMovedEventArgs(this, move));
        if (Square.Digit == _promotionSquareNumber)
        {
            if (IsRaw)
                Owner.OnPromotion(this);
            else
                Promotion?.Invoke(this, EventArgs.Empty);
        }       
    }
}
