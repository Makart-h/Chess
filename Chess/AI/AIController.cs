using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;
using Chess.Board;
using Chess.Movement;
using System.Linq;
using Chess.Positions;

namespace Chess.AI
{
    internal class AIController : Controller
    {
        public AIController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
        {

        }
        public override bool Update()
        {
            if (base.Update())
            {
                ChooseAMove();
                return true;
            }
            else
                return false;
        }
        public override void ChooseAMove()
        {
            (Move mov, double eval)?[] options = new (Move mov, double eval)?[pieces.Count];
            for(int i = 0; i < options.Length; ++i)
            {
                options[i] = ConsiderAPiece(pieces[i]);
            }
            int max = 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (team == Team.White)
                {
                    if (options[i]?.eval > options[max]?.eval || !options[max].HasValue)
                        max = i;
                }
                else
                {
                    if (options[i]?.eval < options[max]?.eval || !options[max].HasValue)
                        max = i;
                }
            }

            if (Chessboard.Instance.MovePiece(pieces[max], options[max].Value.mov.Latter, out Move move))
                OnMoveChosen(new MoveChosenEventArgs(this, pieces[max], move));
            else
                throw new ArgumentOutOfRangeException("AI chose invalid move!");
        }
        private (Move move, double eval)? ConsiderAPiece(Piece piece)
        {
            if (piece.Moves.Count == 0)
                return null;

            double[] evaluations = new double[piece.Moves.Count];

            for(int i = 0; i < evaluations.Length; ++i)
            {
                evaluations[i] = ConsiderAMove(piece.Moves[i]);
            }

            int max = 0;
            for (int i = 0; i < evaluations.Length; i++)
            {
                if (team == Team.White)
                {
                    if (evaluations[i] > evaluations[max])
                        max = i;
                }
                else
                {
                    if (evaluations[i] < evaluations[max])
                        max = i;
                }
            }
            return (piece.Moves[max], evaluations[max]);
        }
        private double ConsiderAMove(Move move)
        {
            return PositionEvaluator.EvaluatePosition(new Position(Chessboard.Instance, team, move));
        }
        public void ChooseAMoveAsync() //needs override!!!
        {
            (Move mov, double eval)?[] options = new (Move mov, double eval)?[pieces.Count];
            Task.WhenAll(Enumerable.Range(0, options.Length).Select(async i => options[i] = await ConsiderAPieceAsync(pieces[i])));
            int max = 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (team == Team.White)
                {
                    if (options[i]?.eval > options[max]?.eval || !options[max].HasValue)
                        max = i;
                }
                else
                {
                    if (options[i]?.eval < options[max]?.eval || !options[max].HasValue)
                        max = i;
                }
            }

            if (Chessboard.Instance.MovePiece(pieces[max], options[max].Value.mov.Latter, out Move move))
                OnMoveChosen(new MoveChosenEventArgs(this, pieces[max], move));
            else
                throw new ArgumentOutOfRangeException("AI chose invalid move!");
        }

        private async Task<(Move move, double eval)?> ConsiderAPieceAsync(Piece piece)
        {
            if(piece.Moves.Count == 0)
                return null;

            double[] evaluations = new double[piece.Moves.Count];

            await Task.WhenAll(Enumerable.Range(0, evaluations.Length).Select(async i => evaluations[i] = await ConsiderAMoveAsync(piece.Moves[i])));
            int max = 0;
            for(int i = 0; i < evaluations.Length; i++)
            {
                if (team == Team.White)
                {
                    if (evaluations[i] > evaluations[max])
                        max = i;
                }
                else
                {
                    if (evaluations[i] < evaluations[max])
                        max = i;
                }
            }
            return (piece.Moves[max], evaluations[max]);
        }

        private Task<double> ConsiderAMoveAsync(Move move)
        {
            return Task.Run(() => PositionEvaluator.EvaluatePosition(new Position(Chessboard.Instance, team, move)));
        }
    }
}
