using Chess.AI;
using Chess.Board;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Positions.Evaluators;

internal static class PositionEvaluator
{
    private static Dictionary<int, double> _cache = new();
    private readonly static object _locker = new();
    private static readonly int _endgameMaterialBoundry;
    private static int s_eventCounter;
    public static void OnMoveMade(object sender, MoveMadeEventArgs e)
    {
        s_eventCounter++;
        if(s_eventCounter % 2 == 0)
            _cache = new Dictionary<int, double>();     
    }
    static PositionEvaluator()
    {
        Controller.MoveMade += OnMoveMade;
        _endgameMaterialBoundry = 10;
    }
    public static double EvaluatePosition(Position position)
    {
        GameResult result = Arbiter.AnalyzeBoard(position.Pieces, position.ActiveTeam, position.HalfMoves, position.OccuredPositions, hash: position.Hash);
        position.Result = result;
        switch (result)
        {
            case GameResult.InProgress:
                break;
            case GameResult.White:
                return 1000;
            case GameResult.Black:
                return -1000;
            case GameResult.ThreefoldRepetition:
                return 0;
            default:
                return 0;
        }
        lock (_locker)
        {
            if (_cache.TryGetValue(position.Hash, out double cachedEval))
                return cachedEval;
        }

        double eval = IteratePieces(position);

        lock (_locker)
        {
            _cache[position.Hash] = eval;
        }
        return eval;
    }
    private static double IteratePieces(Position position)
    {
        int whiteMaterial = 0;
        int blackMaterial = 0;
        double evaluation = 0;
        BoardControlEvaluator boardControlEvaluator = new(position.Pieces, position.EnPassant, position.ActiveTeam);
        PawnStructuresEvaluator pawnStructuresEvaluator = new();
        foreach (var piece in position.Pieces)
        {
            if (piece == null)
                continue;

            Team team = piece.Team;
            if(team == Team.White)
                evaluation += boardControlEvaluator.EvaluatePiece(piece, position.Black);
            else
                evaluation += boardControlEvaluator.EvaluatePiece(piece, position.White);

            int pieceValue = piece.Value;
            evaluation += pieceValue;
            if (piece is not King && piece is not Pawn)
            {
                if (team == Team.White)
                    whiteMaterial += pieceValue;
                else
                    blackMaterial += pieceValue;
            }
            else if (piece is Pawn pawn)
            {
                pawnStructuresEvaluator.AddPawn(pawn);
            }
        }
        bool inEndgame = blackMaterial <= -_endgameMaterialBoundry && whiteMaterial <= _endgameMaterialBoundry;
        evaluation += boardControlEvaluator.EvaluateBoardControl(inEndgame);
        evaluation += EvaluateKingPosition(position.White, boardControlEvaluator, inEndgame);
        evaluation += EvaluateKingPosition(position.Black, boardControlEvaluator, inEndgame);
        evaluation += pawnStructuresEvaluator.EvaluatePawnStructures(inEndgame);
        return evaluation;
    }
    private static double EvaluateKingPosition(King king, BoardControlEvaluator boardControlEvaluator, bool inEndgame)
    {
        const double squaresValue = 0.1;
        const double distanceFromTheCenterPerSquareValue = 0.1;
        const double middleIndex = 4.5;
        double evaluation = 0;
        Square[] squares = king.AdjacentSquares;
        int dangerousSquares = 0;
        int sign = king.Team == Team.White ? 1 : -1;
        foreach (var square in squares)
        {
            if (!Square.Validate(square))
                continue;

            Team team = king.Owner.GetTeamOnSquare(square);
            if (!inEndgame && team == ~king.Team)
            {
                dangerousSquares++;
            }
            else
            {
                SquareControl control = boardControlEvaluator.GetSquareControl(square);
                if (control != null && control.TeamInControl == ~king.Team)
                    dangerousSquares++;
            }
        }
        if (!inEndgame)
        {
            double letterDistance = Math.Abs(middleIndex - (king.Square.Letter - 'a' + 1));
            double digitDistance = Math.Abs(middleIndex - king.Square.Digit);
            double distanceFromTheCenter = letterDistance + digitDistance;
            evaluation += distanceFromTheCenterPerSquareValue * distanceFromTheCenter * sign;
        }

        evaluation -= dangerousSquares * squaresValue * sign;
        return evaluation;
    }   
}
