using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chess.Board;
using Chess.Clock;
using Chess.Movement;
using Chess.Pieces;
using Chess.Positions;

namespace Chess.AI
{
    internal class AIController : Controller
    {
        private readonly List<Move> _movesToConsider;
        private bool _isAlreadyThinking;
        public AIController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
        {
            _movesToConsider = new List<Move>();
            _isAlreadyThinking = false;
        }
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
                _isAlreadyThinking = true;
                _ = MakeMoveAsync();
            }
        }
        public async Task MakeMoveAsync()
        {
            CancellationToken token = ChessClock.GetCancelletionToken(this) ?? CancellationToken.None;
            (double evaluation, int depthOfSearching)[] evaluations = new (double, int)[_movesToConsider.Count];
            try
            {
                await Task.WhenAll(Enumerable.Range(0, evaluations.Length).Select(async i => evaluations[i] = await GetMoveEvaluationAsync(move: _movesToConsider[i], token: token)));
                Move bestMove = PickBestMove(evaluations: evaluations, movesToConsider: _movesToConsider.ToArray());
                if (token.IsCancellationRequested)
                    return;
                ApplyMove(move: bestMove);
            }
            catch(OperationCanceledException)
            {
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }     
        }
        private Move PickBestMove((double evaluation, int depthOfSearching)[] evaluations, Move[] movesToConsider)
        {
            if (evaluations.Length != movesToConsider.Length)
                throw new ArgumentOutOfRangeException("The count of evaluations doesn't match the count of moves!");

            (double, int) bestOutcome = _team == Team.White ? evaluations.Max() : evaluations.Min();
            var bestOptions = new List<Move>();
            for (var i = 0; i < evaluations.Length; ++i)
            {
                if (evaluations[i] == bestOutcome)
                    bestOptions.Add(movesToConsider[i]);
            }
            int moveIndex = 0;

            if (bestOptions.Count < 1)
                throw new ArgumentOutOfRangeException("No best move chosen!");

            else if (bestOptions.Count > 1)
            {
                var rand = new Random();
                moveIndex = rand.Next(0, bestOptions.Count);
            }
            return bestOptions[moveIndex];
        }
        private void ApplyMove(Move move)
        {
            if (Chessboard.Instance.GetAPiece(move.Former, out Piece piece))
            {
                if (Chessboard.Instance.MovePiece(piece, move.Latter, out Move _))
                {
                    OnMoveMade(new MoveMadeEventArgs(this, piece, move));
                    return;
                }
            }
            throw new ArgumentOutOfRangeException("AI didn't manage to chose a valid move!");
        }
        private  Task<(double, int)> GetMoveEvaluationAsync(Move move, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var position = await Position.CreateAsync(board: Chessboard.Instance, activeTeam: _team, move: move, token: token);
                    var node = await PositionNode.CreateAsync(position: position, rank: 2, team: _team, depth: 1, token: token);
                    return await node.FindBestOutcomeAsync(token: token);
                }
                catch(OperationCanceledException)
                {
                    throw;
                }
            }, token);
        }
        protected override void OnMoveMade(MoveMadeEventArgs e)
        {
            _movesToConsider.Clear();
            _isAlreadyThinking = false;
            base.OnMoveMade(e);
        }
    }
}
