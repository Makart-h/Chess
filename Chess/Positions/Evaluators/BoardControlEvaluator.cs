using Chess.Board;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Positions.Evaluators;

internal class BoardControlEvaluator
{
    private readonly SquareControl[] _boardControl;
    private readonly Piece[] _pieces;
    private readonly Square? _enPassantSquare;
    private readonly Team _activeTeam;
    private static readonly byte s_maxSquares = 64;
    private BoardControlInfo _boardControlInfo;
    public BoardControlEvaluator(Piece[] pieces, Square? enPassantSquare, Team activeTeam)
    {
        _boardControl = new SquareControl[s_maxSquares];
        _pieces = pieces;
        _enPassantSquare = enPassantSquare;
        _activeTeam = activeTeam;
        _boardControlInfo = new BoardControlInfo();
    }
    public void AddSquares(Square origin, Square destination)
    {
        if (_boardControl[destination.Index] != null)
        {
            _boardControl[destination.Index].AddSquare(origin, _pieces[origin.Index].Team);
        }
        else
        {
            SquareControl squareControl = new(destination);
            squareControl.AddSquare(origin, _pieces[origin.Index].Team);
            _boardControl[destination.Index] = squareControl;
        }
    }
    public SquareControl GetSquareControl(Square square) => _boardControl[square.Index];
    public double EvaluatePieceMoves(Piece piece, King enemyKing)
    {
        if (piece is Pawn)
            return 0;
        const double developmentValue = 0.3;
        bool isActive = false;
        bool shouldCheckDevelopment = piece is not King and not Queen;
        double evaluation = 0;
        int legalMoves = 0;

        foreach (var move in piece.Moves)
        {
                    AddSquares(move.Former, move.Latter);
                    if (shouldCheckDevelopment)
                    {
                if (enemyKing.IsAdjacentSquare(move.Latter.Letter, move.Latter.Digit) || move.Description == 'x' || move.Description == 'p')
                                isActive = true;

                if(move.Description != 'd')
                                legalMoves++;
                            }
                        }
       
        if (shouldCheckDevelopment)
                        {
            int pieceSign = piece.Team == Team.White ? 1 : -1;
            double factor = legalMoves / (double)GetMovesPotential(piece.Moveset);
            evaluation += developmentValue * pieceSign * factor * (isActive ? 1 : 0.5);
            }
        return evaluation;
        }
    public void GetPawnMoves(Pawn pawn)
        {
        List<Move> takes = new(2);
        Square takesLeft = new(pawn.Square, (-1, pawn.Value));
        Square takesRight = new(takesLeft, (2, 0));
        if (Square.Validate(takesLeft))
            takes.Add(new Move(pawn.Square, takesLeft, 'x'));
        if (Square.Validate(takesRight))
            takes.Add(new Move(pawn.Square, takesRight, 'x'));

        takes = pawn.Owner.GetKing(pawn.Team).FilterMovesThroughThreats(takes);
        foreach (Move move in takes)
        {
            AddSquares(pawn.Square, move.Latter);
        }
    }
    private static int GetMovesPotential(Movesets moveSet)
    {
        return moveSet switch
        {
            Movesets.Queen => 27,
            Movesets.Rook => 14,
            Movesets.Bishop => 13,
            Movesets.Knight => 8,
            Movesets.King => 8,
            Movesets.Pawn => 4,
            _ => 0
        };
    }
    private Piece GetPiece(Square square)
    {
        Piece piece;
        if (square == _enPassantSquare)
        {
            int enPassantPawnDigitDistanceFromEnPassantSquare = _activeTeam == Team.White ? -1 : 1;
            Square enPassantPawnSquare = new(square, (0, enPassantPawnDigitDistanceFromEnPassantSquare));
            piece = _pieces[enPassantPawnSquare.Index];
        }
        else
        {
            piece = _pieces[square.Index];
        }
        return piece;
    }
    private static Team GetTeamFromSign(int sign)
    {
        return sign switch
        {
            -1 => Team.Black,
            1 => Team.White,
            _ => Team.Empty
        };
    }
    public double EvaluateBoardControl(bool inEndgame)
    {
        double evaluation = 0;
        foreach (SquareControl squareControl in _boardControl)
        {
            if (squareControl == null)
                continue;

            int sign = Math.Sign(squareControl.ControlValue);
            Piece piece = GetPiece(squareControl.ControlledSquare);

            if (piece != null)
            {
                double exchangeOutcome = CalculateExchange(piece, squareControl);
                int defendersSign = piece.Team == Team.White ? 1 : -1;
                // It means the exchange went in favour of attackers.
                if (exchangeOutcome > 0)
                {
                    // If it's attackers turn it can simply be captured.
                    if (piece.Team != _activeTeam)
                        evaluation -= exchangeOutcome * defendersSign;
                    sign = -defendersSign;
                }
                else if (exchangeOutcome < 0)
                {
                    sign = defendersSign;
                }
                else
                {
                    sign = 0;
                }
            }
            else if (sign == 0)
                continue;

            _boardControlInfo.IncreaseControls(sign, squareControl.ControlledSquare);
            squareControl.TeamInControl = GetTeamFromSign(sign);
        }
        if (!inEndgame)
            evaluation += _boardControlInfo.Value;
        return evaluation;
    }
    private double CalculateExchange(Piece defendedPiece, SquareControl control)
    {
        Team defendingTeam = defendedPiece.Team;
        List<ExchangeParticipant> defenders = new(2);
        List<ExchangeParticipant> attackers = new(2);
        int defendersValueLost = 0, attackersValueLost = 0;
        var involvedSquares = control.InvolvedSquares;
        for(int i = 0; i < involvedSquares.Count; i++)
        {
            var direction = involvedSquares[i].direction;
            Piece piece = _pieces[involvedSquares[i].square.Index];
            if (piece != null)
            {
                if (piece.Team == defendingTeam)
                    defenders.Add(new(direction, piece));
                else
                    attackers.Add(new(direction, piece));
            }
        }
        // No threats but no control neither.
        if (attackers.Count == 0 && defenders.Count == 0)
            return 0;
        // Defenders control the square.
        else if (attackers.Count == 0 && defenders.Count > 0)
            return -1;

        // Sorting pieces so that the captures are always made with the least valuable piece first.
        SortParticipatingPieces(defenders);
        SortParticipatingPieces(attackers);
        // Defended piece must be inserted at the beggining, because it's the first piece to be captured in the exchange.
        defenders.Insert(0, new((0, 0), defendedPiece));
        Team toMove = ~defendingTeam;
        // Calculating the exchange until the captures are possible/legal or beneficial.
        while (true)
        {
            if (defenders.Count == 0 || attackers.Count == 0)
                break;

            if (toMove == defendingTeam)
            {
                if (!TryPerformExchange(defenders, attackers, ref attackersValueLost))
                    break;
            }
            else
            {
                if (!TryPerformExchange(attackers, defenders, ref defendersValueLost))
                    break;
            }
            toMove = ~toMove;
        }
        // If value is greater than 0 it means the defenders lost on the exchange.
        return Math.Abs(defendersValueLost) - Math.Abs(attackersValueLost);
    }
    private bool TryPerformExchange(List<ExchangeParticipant> activePieces, List<ExchangeParticipant> passivePieces, ref int passiveValueLost)
    {
        var activeFirst = activePieces[0];
        var passiveFirst = passivePieces[0];
        // If the attacking piece is more valuable than the defended piece and would be recaptured, capture is illogical.
        if (passivePieces.Count != 1 && Math.Abs(activeFirst.Piece.Value) > Math.Abs(passiveFirst.Piece.Value))
            return false;

        if (activeFirst.Piece is King && passivePieces.Count > 1)
            return false;

        passiveValueLost += passiveFirst.Piece.Value;
        passivePieces.RemoveAt(0);
        // Check if active piece uncovered a new piece to join the exchange (for either team).
        Movesets approximateMoveSet = GetMoveSetFromDirection(activeFirst.Direction);
        // If the capturing piece was a knight there is no need to check if something was uncovered since there won't be a straight line between the sqaure and the uncovered piece.
        if (approximateMoveSet != Movesets.Knight)
        {
            SquareControl control = _boardControl[activeFirst.Piece.Square.Index];
            if (control != null)
            {
                Square? involvedSquare = control.GetInvolvedSquare(activeFirst.Direction);
                if (involvedSquare.HasValue)
                {
                    Piece potentialInvoledPiece = _pieces[involvedSquare.Value.Index];
                    if (potentialInvoledPiece != null && (potentialInvoledPiece.Moveset & approximateMoveSet) != 0)
                {
                    if (potentialInvoledPiece.Team == activeFirst.Piece.Team)
                    {
                            activePieces.Remove(activeFirst);
                        activePieces.Add(new(activeFirst.Direction, potentialInvoledPiece));
                        SortParticipatingPieces(activePieces);
                            activePieces.Insert(0, activeFirst);
                    }
                    else
                    {
                        passivePieces.Add(new(activeFirst.Direction, potentialInvoledPiece));
                        SortParticipatingPieces(passivePieces);
                    }
                }
            }
        }
        }
        return true;
    }
    private static void SortParticipatingPieces(List<ExchangeParticipant> participatingPieces)
    {
        participatingPieces.Sort();
    }
    private static Movesets GetMoveSetFromDirection((int x, int y) direction)
    {
        if (direction.x == 0)
            return Movesets.Vertical;
        else if (direction.y == 0)
            return Movesets.Horizontal;
        else if (Math.Abs(direction.x) == Math.Abs(direction.y))
            return Movesets.Diagonal;
        else
            return Movesets.Knight;
    }
}
