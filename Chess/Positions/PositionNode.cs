using Chess.AI;
using Chess.Movement;
using Chess.Pieces.Info;
using Chess.Positions.Evaluators;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Positions;

internal sealed class PositionNode
{
    private readonly double _value;
    private readonly int _depth;
    private readonly Team _team;
    private readonly bool _isFertile;
    private List<PositionNode> _children;
    private readonly string _path;
    private static readonly int s_minDepth = 3;
    private static readonly int s_maxDepth = 5;

    private PositionNode(Team team, int depth, string path, Position position, bool isFertile)
    {
        _team = team;
        _depth = depth;
        _value = PositionEvaluator.EvaluatePosition(position);
        if (path != string.Empty)
            path += "|";
        _path = path + position.MovePlayed;
        _isFertile = (depth < s_minDepth || isFertile) && depth < s_maxDepth;
    }
    public static async Task<PositionNode> CreateAsync(string path, Position position, Team team, int depth, bool isFertile, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        PositionNode node = new(team, depth, path, position, isFertile);
        await node.CreateChildrenAsync(position, token);
        return node;
    }
    private async Task CreateChildrenAsync(Position position, CancellationToken token)
    {
        if (_isFertile && position.Result == GameResult.InProgress)
        {
            _children = new(position.NextMoves.Count);
            foreach (Move move in position.NextMoves)
            {
                token.ThrowIfCancellationRequested();
                Position pos = await Position.CreateAsync(position, move, token);
                bool isChildFertile = move.Description == 'x' || move.Description == 'p' || position.Check || pos.Check;
                PositionNode node = await CreateAsync(_path, pos, ~_team, _depth + 1, isChildFertile, token);
                _children.Add(node);
            }
        }         
    }
    private async Task CreateChildrenAsync2(Position position, CancellationToken token)
    {
        if (_isFertile && position.Result == GameResult.InProgress)
        {
            _children = new(position.NextMoves.Count);
            int sign = _team == Team.White ? -1 : 1;
            PositionNode bestNode = null;
            Position bestPosition = null;
            foreach (Move move in position.NextMoves)
            {
                token.ThrowIfCancellationRequested();
                Position pos = await Position.CreateAsync(position, move, token);
                bool isChildFertile = move.Description == 'x' || move.Description == 'p' || position.Check || pos.Check;
                PositionNode node = new(~_team, _depth + 1, _path, pos, isChildFertile);
                if (bestNode == null || node._value.CompareTo(bestNode._value) * sign == 1)
                {
                    bestNode = node;
                    bestPosition = pos;
                }
                if (node._value.CompareTo(_value) * sign != -1)
                {
                    await node.CreateChildrenAsync2(pos, token);
                    _children.Add(node);
                }
            }
            if (_children.Count == 0 && bestNode != null)
            {
                await bestNode.CreateChildrenAsync2(bestPosition, token);
                _children.Add(bestNode);
            }
        }
    }
    public async Task<Evaluation> FindBestOutcomeAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        bool standardComparison = _team == Team.Black;
        if (_children?.Count > 0)
        {
            Evaluation best = await _children[0].FindBestOutcomeAsync(token);
            for (int i = 1; i < _children.Count; ++i)
            {
                Evaluation pretender = await _children[i].FindBestOutcomeAsync(token);
                if (pretender.CompareTo(best, standardComparison) == 1)
                    best = pretender;
            }
            return best;
        }
        else
        {         
            return new Evaluation(_value, _depth, _path);
        }
    }
}
