using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Chess.Movement;
using Chess.Board;
using Chess.Pieces;
using Chess.Positions;

namespace Chess.AI
{
    internal static class PositionEvaluator
    {
        public static double EvaluatePosition(Position position)
        {
            Piece[] pieces = (from p in position.Pieces.Values where p != null select p).ToArray();
            double eval = 0;
            eval += GetPiecesValues(pieces);
            eval += GetCenterControl(pieces);
            eval += GetMovesCounts(pieces);
            return eval;
        }
        private static int GetPiecesValues(Piece[] pieces) => pieces.Sum(p => p.Value);
        private static double GetCenterControl(Piece[] pieces)
        {
            double sum = 0;
            Square[] centre = { new Square("d4"), new Square("d5"), new Square("e5"), new Square("e5") };
            foreach(Piece piece in pieces)
            {
                foreach(Move move in piece.Moves)
                {
                    if (centre.Contains(move.Latter))
                        sum += piece.Team == Team.White ? 0.5 : -0.5;
                }
            }
            return sum;
        }
        private static double GetMovesCounts(Piece[] pieces)
        {
            int white = 0;
            int black = 0;
            double eval;
            foreach (var piece in pieces)
            {
                if (piece.Team == Team.White)
                    white += piece.Moves.Count;
                else
                    black += piece.Moves.Count;
            }
            if (white == 0)
                eval = -1000;
            else if (black == 0)
                eval = 1000;
            else
                eval = 0.1 * white - 0.1 * black;
            
            return eval;
        }
    }  
}
