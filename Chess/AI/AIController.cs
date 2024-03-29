﻿using Chess.Board;
using Chess.Clock;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Chess.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.AI;

internal sealed class AIController : Controller
{
    private readonly List<Move> _movesToConsider;
    private bool _isAlreadyThinking;
    private readonly List<string> _movesQueue;
    public AIController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
    {
        _movesToConsider = new();
        _isAlreadyThinking = false;
        _movesQueue = new();
        MoveMade += OnOpponentMoveMade;
    }
    public override IEnumerable<IDrawable> GetDrawableObjects() => _piecesModels.Values;
    public override void Update()
    {
        base.Update();
        foreach (Piece piece in _pieces)
            _movesToConsider.AddRange(piece.Moves);
    }
    public override void MakeMove()
    {
        if (!_isAlreadyThinking)
        {
            if (!TryTakeMoveFromQueue())
            {
                _isAlreadyThinking = true;
                _ = MakeMoveAsync();
            }
        }
    }
    public async Task MakeMoveAsync()
    {
        CancellationToken token = ChessClock.GetCancelletionToken(this) ?? CancellationToken.None;
        Evaluation[] evaluations = new Evaluation[_movesToConsider.Count];
        try
        {
            await Task.WhenAll(Enumerable.Range(0, evaluations.Length).Select(async i => evaluations[i] = await GetMoveEvaluationAsync(move: _movesToConsider[i], token: token)));
            Move bestMove = PickBestMove(evaluations: evaluations, movesToConsider: _movesToConsider.ToArray());
            if (token.IsCancellationRequested)
                return;
            ApplyMove(move: bestMove);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw;
        }
    }
    private Move PickBestMove(Evaluation[] evaluations, Move[] movesToConsider)
    {
        if (evaluations.Length != movesToConsider.Length)
            throw new ArgumentOutOfRangeException("The count of evaluations doesn't match the count of moves!");

        Evaluation bestOutcome = Evaluation.Max(evaluations, Team == Team.White);
        var bestOptions = new List<int>();
        for (var i = 0; i < evaluations.Length; ++i)
        {
            if (evaluations[i] == bestOutcome)
                bestOptions.Add(i);
        }
        int index = bestOptions[GetIndexOfBestOption(bestOptions.ToArray())];
        RefreshMovesQueue(evaluations[index]);
        return movesToConsider[index];
    }
    private void RefreshMovesQueue(Evaluation chosenEvaluation)
    {
        double desiredValue = Team == Team.White ? 1000 : -1000;
        if (_movesQueue.Count == 0 && chosenEvaluation.Value == desiredValue)
        {
            _movesQueue.AddRange(chosenEvaluation.Path.Split("|"));
            _movesQueue.RemoveAt(0);
        }
    }
    private static int GetIndexOfBestOption(int[] bestOptions)
    {
        int moveIndex = 0;
        if (bestOptions.Length < 1)
            throw new ArgumentOutOfRangeException("There are no moves to choose!");

        else if (bestOptions.Length > 1)
        {
            var rand = new Random();
            moveIndex = rand.Next(0, bestOptions.Length);
        }
        return moveIndex;
    }
    private void ApplyMove(Move move)
    {
        Piece piece = Chessboard.Instance.GetPiece(move.Former);
        if (piece != null)
        {
            if (Chessboard.Instance.MovePiece(piece, move.Latter, out Move _))
            {
                OnMoveMade(new MoveMadeEventArgs(this, piece, move));
                return;
            }
        }
        throw new ArgumentOutOfRangeException("AI didn't manage to chose a valid move!");
    }
    private bool TryTakeMoveFromQueue()
    {
        if(_movesQueue.Count > 0)
        {
            string queuedMove = _movesQueue.First();
            _movesQueue.RemoveAt(0);
            string formerSquare = queuedMove[..2];
            Square former = new(formerSquare);
            MoveType description = (MoveType)int.Parse(queuedMove[2].ToString());
            string latterSquare = queuedMove[3..5];
            Square latter = new(latterSquare);
            Move move = new(former, latter, description);
            ApplyMove(move);
            return true;
        }
        return false;
    }
    private async Task<Evaluation> GetMoveEvaluationAsync(Move move, CancellationToken token)
    {
        var position = await Position.CreateAsync(board: Chessboard.Instance, activeTeam: Team, move: move, token: token, occuredPositions: Arbiter.OccuredPositions);
        token.ThrowIfCancellationRequested();
        var node = await PositionNode.CreateAsync(path: string.Empty, position: position, team: Team, depth: 1, isFertile: true, token: token);
        token.ThrowIfCancellationRequested();
        return await node.FindBestOutcomeAsync(token: token);
    }
    protected override void OnMoveMade(MoveMadeEventArgs e)
    {
        _movesToConsider.Clear();
        _isAlreadyThinking = false;
        base.OnMoveMade(e);
    }
    private void OnOpponentMoveMade(object sender, MoveMadeEventArgs e)
    {
        if (e.Controller != this && _movesQueue.Count > 0)
        {
            string opponentMove = $"{e.Move.Former}{e.Move.Description}{e.Move.Latter}";
            if (_movesQueue.First().Contains(opponentMove))
                _movesQueue.RemoveAt(0);
            else
                _movesQueue.Clear();
        }
    }
}
