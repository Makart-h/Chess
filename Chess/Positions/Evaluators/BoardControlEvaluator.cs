using Chess.Board;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Positions.Evaluators;

internal class BoardControlEvaluator
{
    private readonly Dictionary<Square, SquareControl> _boardControl;
    private readonly Dictionary<Square, Piece> _pieces;
    private readonly Square? _enPassantSquare;
    private readonly Team _activeTeam;
    private BoardControlInfo _boardControlInfo;
    public BoardControlEvaluator(Dictionary<Square, Piece> pieces, Square? enPassantSquare, Team activeTeam)
    {
        _boardControl = new();
        _pieces = pieces;
        _enPassantSquare = enPassantSquare;
        _activeTeam = activeTeam;
        _boardControlInfo = new BoardControlInfo();
    }
    public void AddSquares(Square origin, Square destination)
    {
        if (_boardControl.ContainsKey(destination))
        {
            _boardControl[destination].AddSquare(origin, _pieces[origin].Team);
        }
        else
        {
            SquareControl squareControl = new(destination);
            squareControl.AddSquare(origin, _pieces[origin].Team);
            _boardControl[destination] = squareControl;
        }
    }
    public SquareControl GetSquareControl(Square square)
    {
        if (_boardControl.ContainsKey(square))
            return _boardControl[square];
        else
            return null;
    }
    public double EvaluatePieceMoves(Piece piece)
    {
        const double developmentValue = 0.3;
        int pieceSign = piece.Team == Team.White ? 1 : -1;
        Square[] squaresAroundEnemyKing = piece.Owner.GetKing(~piece.Team).GetSquaresAroundTheKing();
        bool isActive = false;
        bool shouldCheckDevelopment = piece is not King and not Pawn;
        double evaluation = 0;
        int legalMoves = 0;

        foreach (var (_, moves) in MovementManager.GenerateEveryMove(piece.Square, piece.MoveSet, piece.Owner, friendlyFire: true))
        {
            foreach (var move in moves)
            {
                if (piece.Owner.GetKing(piece.Team).CheckMoveAgainstThreats(piece, move))
                {
                    if (squaresAroundEnemyKing.Contains(move.Latter))
                        isActive = true;

                    AddSquares(move.Former, move.Latter);
                    if (shouldCheckDevelopment)
                    {
                        if (move.Description == 'x' || move.Description == 'p')
                        {
                            if (_pieces[move.Latter].Team != piece.Team)
                            {
                                isActive = true;
                                legalMoves++;
                            }
                        }
                        else
                        {
                            legalMoves++;
                        }
                    }
                }
            }
        }
        if (shouldCheckDevelopment)
        {
            double factor = legalMoves / (double)GetMovesPotential(piece.MoveSet);
            evaluation += developmentValue * pieceSign * factor * (isActive ? 1 : 0.75);
        }
        return evaluation;
    }
    private static int GetMovesPotential(MoveSets moveSet)
    {
        return moveSet switch
        {
            MoveSets.Queen => 27,
            MoveSets.Rook => 14,
            MoveSets.Bishop => 13,
            MoveSets.Knight => 8,
            MoveSets.King => 8,
            MoveSets.Pawn => 4,
            _ => 0
        };
    }
    private Piece GetPiece(Square square)
    {
        Piece piece;
        if (square == _enPassantSquare)
        {
            int enPassantPawnDigitDistanceFromEnPassantSquare = _activeTeam == Team.White ? -1 : 1;
            Square enPassantPawnSquare = new(square.Letter, square.Digit + enPassantPawnDigitDistanceFromEnPassantSquare);
            piece = _pieces[enPassantPawnSquare];
        }
        else
        {
            piece = _pieces[square];
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
        foreach (Square square in _boardControl.Keys)
        {
            int sign = Math.Sign(_boardControl[square].ControlValue);
            Piece piece = GetPiece(square);

            if (piece != null)
            {
                double exchangeOutcome = CalculateExchange(piece, _boardControl[square].InvolvedSquares);
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

            _boardControlInfo.IncreaseControls(sign, square);
            _boardControl[square].TeamInControl = GetTeamFromSign(sign);
        }
        if (!inEndgame)
            evaluation += _boardControlInfo.Value;
        return evaluation;
    }
    private double CalculateExchange(Piece defendedPiece, Dictionary<(int, int), Square> involvedSquares)
    {
        Team defendingTeam = defendedPiece.Team;
        List<ExchangeParticipant> defenders = new();
        List<ExchangeParticipant> attackers = new();
        int defendersValueLost = 0, attackersValueLost = 0;
        foreach ((int, int) direction in involvedSquares.Keys)
        {
            if (_pieces.TryGetValue(involvedSquares[direction], out Piece piece))
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
        var activeFirst = activePieces.First();
        var passiveFirst = passivePieces.First();
        // If the attacking piece is more valuable than the defended piece and would be recaptured, capture is illogical.
        if (passivePieces.Count != 1 && Math.Abs(activeFirst.Piece.Value) > Math.Abs(passiveFirst.Piece.Value))
            return false;

        if (activeFirst.Piece is King && passivePieces.Count > 1)
            return false;

        passiveValueLost += passiveFirst.Piece.Value;
        passivePieces.RemoveAt(0);
        // Check if active piece uncovered a new piece to join the exchange (for either team).
        MoveSets approximateMoveSet = GetMoveSetFromDirection(activeFirst.Direction);
        // If the capturing piece was a knight there is no need to check if something was uncovered since there won't be a straight line between the sqaure and the uncovered piece.
        if (approximateMoveSet != MoveSets.Knight && _boardControl.TryGetValue(activeFirst.Piece.Square, out SquareControl control))
        {
            if (control.TryGetInvolvedSquare(activeFirst.Direction, out Square involvedSquare))
            {
                if (_pieces.TryGetValue(involvedSquare, out Piece potentialInvoledPiece) && potentialInvoledPiece != null && (potentialInvoledPiece.MoveSet & approximateMoveSet) != 0)
                {
                    if (potentialInvoledPiece.Team == activeFirst.Piece.Team)
                    {
                        activePieces.Add(new(activeFirst.Direction, potentialInvoledPiece));
                        SortParticipatingPieces(activePieces);
                    }
                    else
                    {
                        passivePieces.Add(new(activeFirst.Direction, potentialInvoledPiece));
                        SortParticipatingPieces(passivePieces);
                    }
                }
            }
        }
        return true;
    }
    private static void SortParticipatingPieces(List<ExchangeParticipant> participatingPieces)
    {
        participatingPieces.Sort();
        // If either side has a king involved in the exchange we must move it to the end, since you can't caputre with the king on a threatened square.
        var first = participatingPieces.FirstOrDefault();
        if (first.Piece is King)
        {
            participatingPieces.Remove(first);
            participatingPieces.Add(first);
        }
    }
    private static MoveSets GetMoveSetFromDirection((int x, int y) direction)
    {
        if (direction.x == 0)
            return MoveSets.Vertical;
        else if (direction.y == 0)
            return MoveSets.Horizontal;
        else if (Math.Abs(direction.x) == Math.Abs(direction.y))
            return MoveSets.Diagonal;
        else
            return MoveSets.Knight;
    }
}
