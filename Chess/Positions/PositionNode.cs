using Chess.AI;
using Chess.Movement;
using Chess.Pieces.Info;
using Chess.Positions.Evaluators;
using System.Collections.Generic;
using System.Linq;
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
    private readonly List<PositionNode> _children;
    private readonly StringBuilder _path;
    private static readonly int s_minDepth = 2;
    private static readonly int s_maxDepth = 4;

    private PositionNode(Team team, int depth, string path, Position position, bool isFertile)
    {
        _team = team;
        _depth = depth;
        _children = new();
        _path = new(path);      
        _value = PositionEvaluator.EvaluatePosition(position);
        _path.Append($"{(_path.Length > 0 ? "->" : "")}{position.MovePlayed}({_value:0.00})");
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
            foreach (Move move in position.NextMoves)
            {
                token.ThrowIfCancellationRequested();
                Position pos = await Position.CreateAsync(position, move, token);
                bool isChildFertile = move.Description == 'x' || move.Description == 'p' || position.Check || pos.Check;
                PositionNode node = await CreateAsync(_path.ToString(), pos, ~_team, _depth + 1, isChildFertile, token);
                _children.Add(node);
            }
        }         
    }
    public async Task<Evaluation> FindBestOutcomeAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (_children.Count > 0)
        {
            Evaluation[] evaluations = new Evaluation[_children.Count];
            for (int i = 0; i < _children.Count; i++)
            {
                evaluations[i] = await _children[i].FindBestOutcomeAsync(token);
            }
            if (_team == Team.Black)
                return evaluations.Max();
            else
                return evaluations.Min();
        }
        else
        {
            int sign = _team == Team.White ? -1 : 1;
            return new Evaluation(_value, sign * _depth, _path.ToString());
        }
    }
}
