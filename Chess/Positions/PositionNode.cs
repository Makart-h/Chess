using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Chess.Pieces;
using Chess.AI;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Chess.Movement;

namespace Chess.Positions
{
    internal class PositionNode
    {
        private double value;
        private readonly int depth;
        private readonly Team team;
        private readonly List<PositionNode> children;

        private PositionNode(Team team, int depth)
        {
            this.team = team;
            this.depth = depth;
            children = new List<PositionNode>();
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
                foreach (var move in position.NextMoves)
                {
                    token.ThrowIfCancellationRequested();
                    Position pos = await Position.CreateAsync(token, position, move);
                    PositionNode node = await CreateAsync(pos, rank - (position.Check ? 0 : 1), ~team, depth + 1, token);
                    children.Add(node);
                }
            }
            else
                value = PositionEvaluator.EvaluatePosition(position);
        }
        public async Task<(double, int)> FindBestOutcomeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (children?.Count > 0)
            {
                (double eval, int depth)[] evals = new (double, int)[children.Count];
                for (int i = 0; i < children.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    int index = i;
                    evals[index] = await children[index].FindBestOutcomeAsync(token);
                }
                if (team == Team.Black)
                {
                    return evals.Max();
                }
                else
                {
                    return evals.Min();
                }
            }
            else
                return team == Team.White ? (value, -depth) : (value, depth);
        }
    }
}
