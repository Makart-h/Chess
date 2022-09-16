using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Chess.AI;
using Chess.Movement;
using Chess.Pieces;

namespace Chess.Positions
{
    internal sealed class PositionNode
    {
        private double _value;
        private readonly int _depth;
        private readonly Team _team;
        private readonly List<PositionNode> _children;

        private PositionNode(Team team, int depth)
        {
            _team = team;
            _depth = depth;
            _children = new List<PositionNode>();
        }
        public static async Task<PositionNode> CreateAsync(Position position, int rank, Team team, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            PositionNode node = new PositionNode(team, depth);
            await node.CreateChildrenAsync(position, rank, token);
            return node;
        }
        private async Task CreateChildrenAsync(Position position, int rank, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (rank > 0 && position.NextMoves.Count > 0)
            {
                foreach (Move move in position.NextMoves)
                {
                    token.ThrowIfCancellationRequested();
                    Position pos = await Position.CreateAsync(token, position, move);
                    PositionNode node = await CreateAsync(pos, rank - (position.Check ? 0 : 1), ~_team, _depth + 1, token);
                    _children.Add(node);
                }
            }
            else
                _value = PositionEvaluator.EvaluatePosition(position);
        }
        public async Task<(double, int)> FindBestOutcomeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_children?.Count > 0)
            {
                (double eval, int depth)[] evals = new (double, int)[_children.Count];
                for (int i = 0; i < _children.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    int index = i;
                    evals[index] = await _children[index].FindBestOutcomeAsync(token);
                }
                if (_team == Team.Black)
                {
                    return evals.Max();
                }
                else
                {
                    return evals.Min();
                }
            }
            else
                return _team == Team.White ? (_value, -_depth) : (_value, _depth);
        }
    }
}
