using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Chess.Movement;
using Chess.Board;
using Chess.Pieces;
using Chess.Positions;
using System.Threading;
using Microsoft.Xna.Framework.Media;

namespace Chess.AI
{
    internal static class PositionEvaluator
    {
        private static Dictionary<string, double> cache = new Dictionary<string, double>();
        private readonly static object locker = new object();
        private static readonly Square[] center;
        private static readonly Square[] smallCenter;
        private static readonly int endgameMaterialBoundry;
        public static void OnMoveMade(object sender, MoveMadeEventArgs e) => cache = new Dictionary<string, double>();
        static PositionEvaluator()
        {
            Controller.MoveMade += OnMoveMade;
            center = new[] { new Square("E4"), new Square("E5"), new Square("D4"), new Square("D5") };
            smallCenter = new[] { new Square("E3"), new Square("E6"), new Square("D3"), new Square("D6"),
            new Square("C6"), new Square("C5"), new Square("C4"), new Square("C3"),
            new Square("F6"), new Square("F5"), new Square("F4"), new Square("F3") };
            endgameMaterialBoundry = 9;
        }
        public static double EvaluatePosition(Position position)
        {
            lock (locker)
            {
                if (cache.TryGetValue(position.ShortFEN, out double cachedEval))
                    return cachedEval;
            }
            position.PrepareForEvaluation();
            int count = GetMoveCounts(position);
            switch (count)
            {
                case -1:
                    return -1000;
                case 0:
                    return 0;
                case 1:
                    return 1000;
            }
            double eval = IteratePieces(position);
            lock (locker)
            {
                cache[position.ShortFEN] = eval;
            }
            return eval;
        }
        private static double IteratePieces(Position position)
        {
            int whiteMaterial = 0;
            int blackMaterial = 0;
            int whitePieces = 0;
            int blackPieces = 0;
            double eval = 0;
            Dictionary<Square, (int, List<Piece>)> boardControl = new Dictionary<Square, (int, List<Piece>)>();
            var pawnDicitonary = GetPawnDictionary();
            foreach (var piece in position.Pieces.Values)
            {
                if (piece == null)
                    continue;

                eval += GetPieceMoves(position, piece, boardControl);
                int value = GetPieceValue(piece);
                eval += value;
                if (!(piece is King) && !(piece is Pawn))
                {
                    if (piece.Team == Team.White)
                        whiteMaterial += value;
                    else
                        blackMaterial += Math.Abs(value);
                }
                if (piece is Pawn p)
                {
                    pawnDicitonary[p.Square.Number.letter][p.Square.Number.digit - 1] = p.Team == Team.White ? 'P' : 'p';
                }
                if (piece.Team == Team.White)
                    whitePieces++;
                else
                    blackPieces++;
            }         
            bool endgame = blackMaterial < endgameMaterialBoundry;
            eval += CheckKingSafety(position.White, boardControl, endgame);
            endgame = whiteMaterial < endgameMaterialBoundry;
            eval += CheckKingSafety(position.Black, boardControl, endgame);
            eval += CheckBoardControl(boardControl, position.Pieces, position.ActiveTeam, whitePieces, blackPieces, blackMaterial < endgameMaterialBoundry && whiteMaterial < endgameMaterialBoundry);

            double pawnValues = CheckPawnStructers(pawnDicitonary);
            eval += pawnValues;
            return eval;
        }
        private static Dictionary<char, char[]> GetPawnDictionary()
        {
            Dictionary<char, char[]> dict = new Dictionary<char, char[]>
            {
                ['A'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['B'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['C'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['D'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['E'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['F'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['G'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' },
                ['H'] = new char[] { 'x', 'x', 'x', 'x', 'x', 'x', 'x', 'x' }
            };
            return dict;
        }
        private static int GetMoveCounts(Position position)
        {
            if (position.NextMoves.Count == 0)
            {
                if (position.ActiveTeam == Team.White)
                {
                    if (position.White.Threatened)
                        return -1;
                    else
                        return 0;
                }
                else if (position.Black.Threatened)
                    return 1;
                else
                    return 0;
            }
            return 2;
        }
        private static int GetPieceValue(Piece piece) => piece != null ? piece.Value : 0;
        private static double GetPieceMoves(Position position, Piece piece, Dictionary<Square, (int control, List<Piece> pieces)> boardControl)
        {
            if (piece == null)
                return 0;

            const double moveValue = 0.5;
            const int fenomenalNumberOfMoves = 5;
            int pieceSign = piece.Team == Team.White ? 1 : -1;
            double eval = 0;
            List<Move> movesWithFriendlyFire = new List<Move>();

            foreach (var groupedMoves in Move.GenerateEveryMove(piece.Square, piece.MoveSet, position, friendlyFire: true))
                movesWithFriendlyFire.AddRange(groupedMoves.moves);

            int moves = (!(piece is King) && !(piece is Pawn)) ? movesWithFriendlyFire.Count : 0;

            foreach (var move in movesWithFriendlyFire)
            {
                if (boardControl.TryGetValue(move.Latter, out (int control, List<Piece> pieces) value))
                {
                    value.pieces.Add(piece);
                    boardControl[move.Latter] = (value.control + pieceSign, value.pieces);
                }
                else
                {
                    List<Piece> freshList = new List<Piece> { piece };
                    boardControl[move.Latter] = (pieceSign, freshList);
                }
                if (moves > 0 && move.Description == 'x' && position.Pieces[move.Latter].Team == piece.Team)
                    moves--;
            }
            moves = Math.Clamp(moves, 0, fenomenalNumberOfMoves);
            eval += moveValue * (moves/fenomenalNumberOfMoves) * pieceSign;
            return eval;
        }
        private static double CheckBoardControl(Dictionary<Square, (int control, List<Piece> pieces)> boardControl, Dictionary<Square, Piece> pieces, Team toMove, int whitePieces, int blackPieces, bool endgame)
        {          
            double eval = 0;
            const double centerValue = 0.5;
            const double weakPieceValue = 0.3;
            const double smallCenterValue = 0.1;
            (double white, double black) undefendedPieces = (whitePieces, blackPieces);
            (int small, int regular) centerControl = (0,0);
            foreach (var square in boardControl.Keys)
            {
                int value = boardControl[square].control;
                int sign = Math.Sign(value);

                if (pieces.TryGetValue(square, out Piece piece) && piece != null)
                {
                    double exchangeOutcome = CalculateTheExchange(piece, boardControl[square].pieces);
                    int defendersSign = piece.Team == Team.White ? 1 : -1;

                    // It means the exchange went in favour of attackers.
                    if (exchangeOutcome > 0)
                    {
                        // If it's attackers turn it can be simply captured.
                        if (piece.Team == Team.White && toMove == Team.Black || piece.Team == Team.Black && toMove == Team.White)
                        {
                            eval -= exchangeOutcome * defendersSign;                           
                        }
                        sign = -defendersSign;
                        if (piece.Team == Team.Black)
                            undefendedPieces.black--;
                        else
                            undefendedPieces.white--;
                    }
                    else if (exchangeOutcome < 0)
                    {
                        if(piece.Team == Team.Black)
                        {
                            undefendedPieces.black--;
                            sign = -1;
                        }
                        else
                        {
                            undefendedPieces.white--;
                            sign = 1;
                        }
                    }
                    else
                    {
                        sign = 0;
                        if (piece.Team == Team.Black)
                            undefendedPieces.black--;
                        else
                            undefendedPieces.white--;
                    }
                }
                else if (value == 0)
                    continue;

                
                if (center.Contains(square))
                    centerControl.regular += sign;
                else if (smallCenter.Contains(square))
                    centerControl.small += sign;

            }
            if (!endgame)
            {
                eval += centerValue * Math.Sign(centerControl.regular);
                eval += smallCenterValue * Math.Sign(centerControl.small);
            }
            eval += undefendedPieces.black * weakPieceValue;
            eval -= undefendedPieces.white * weakPieceValue;
            return eval;
        }
        private static double CalculateTheExchange(Piece defendedPiece, List<Piece> fightingPieces)
        {
            Team defendingTeam = defendedPiece.Team;
            List<Piece> defenders = new List<Piece>();
            List<Piece> attackers = new List<Piece>();
            int defendersValueLost = 0, attackersValueLost = 0;
            foreach(var piece in fightingPieces)
            {
                if (piece.Team == defendingTeam)
                {
                    defenders.Add(piece);
                }
                else
                {
                    attackers.Add(piece);
                }
            }
            // Undefended piece can be simply captured.
            if (attackers.Count > 0 && defenders.Count == 0)
                return Math.Abs(defendedPiece.Value);
            // Nothing to defend.
            else if (attackers.Count == 0)
                return 0;

            // Sorting pieces so that the captures are always made with the least valuable piece first.
            defenders.Sort();
            attackers.Sort();

            // If either side has a king involved in the exchange we must move it to the end, since you can't caputre with the king on a threatened square.
            if (defenders.First() is King dk)
            {
                defenders.Remove(dk);
                defenders.Add(dk);
            }
            if(attackers.First() is King ak)
            {
                attackers.Remove(ak);
                attackers.Add(ak);
            }
            // Defended piece must be inserted at the beggining, because it's the first piece to be captured in the exchange.
            defenders.Insert(0, defendedPiece);
            Team toMove = ~defendingTeam;
            // Calculating the exchange until the captures are possible/legal or beneficial.
            while(true)
            {
                if (defenders.Count == 0 || attackers.Count == 0)
                    break;

                if(toMove == defendingTeam)
                {
                    // If the attacking piece is more valuable then the defended piece and would be recaptured, capture is illogical.
                    if (attackers.Count != 1 && defenders.First().Value > attackers.First().Value)
                        break;

                    if (defenders.First() is King && attackers.Count > 1)
                        break;
                    attackersValueLost += attackers.First().Value;
                    attackers.RemoveAt(0);
                }
                else
                {
                    // If the attacking piece is more valuable then the defended piece and would be recaptured, capture is illogical.
                    if (defenders.Count != 1 && attackers.First().Value > defenders.First().Value)                       
                        break;

                    if (attackers.First() is King && defenders.Count > 1)
                        break;

                    defendersValueLost += defenders.First().Value;
                    defenders.RemoveAt(0);
                }
                toMove = ~toMove;
            }
            // If value is greater than 0 it means the defenders lost on the exchange.
            return Math.Abs(defendersValueLost) - Math.Abs(attackersValueLost);
        }
        private static double CheckKingSafety(King king, Dictionary<Square, (int control, List<Piece> pieces)> boardControl, bool endgame)
        {
            const double squaresValue = 0.5;
            const double castlingSquaresValue = 0.4;
            Square[] squares = king.GetSquaresAroundTheKing();
            int dangerousSquares = 0;
            double eval = 0;
            int sign = (king.Team == Team.White ? 1 : -1);
            foreach (var square in squares)
            {
                if (!Square.Validate(square))
                    continue;

                Team team = king.Owner.IsSquareOccupied(square);
                if (team == Team.Empty || team == ~king.Team)
                    dangerousSquares++;

                else if (boardControl.TryGetValue(square, out var value))
                {
                    if (value.control == 0 || Math.Sign(value.control) != sign)
                        dangerousSquares++;
                }
            }
            if (!endgame)
            {
                List<Square> castlingSquares = new List<Square>();
                castlingSquares.AddRange(king.GetKingSideCastleSquares());
                castlingSquares.AddRange(king.GetQueenSideCastleSquares());
                castlingSquares.Add(King.GetCastlingRookSquare('k', king.Team));
                castlingSquares.Add(King.GetCastlingRookSquare('q', king.Team));

                if (castlingSquares.Contains(king.Square))
                    eval += castlingSquaresValue * sign;
            }
            eval -= dangerousSquares * squaresValue * sign;
            return eval;
        }
        private static double CheckPawnStructers(Dictionary<char, char[]> files)
        {
            const double doubledPawnsValue = 0.5;
            const double isolatedPawnValue = 0.5;
            const double pawnNumberAdvantageValue = 0.3;
            const double rankValue = 0.2;
            const double passedPawnValue = 0.5;
            const int minRank = 2;
            const double nearPromotionValue = 3.0;
            const int nearPromitionRank = 7;
            int kingsideNumber = 0;
            int queensideNumber = 0;
            double eval = 0;
            foreach (var key in files.Keys)
            {
                int whitePawns = 0;
                int blackPawns = 0;
                for (int i = 0; i < files[key].Length; ++i)
                {
                    if (files[key][i] == 'P')
                    {
                        int rank = i + 1;
                        if (rank == nearPromitionRank)
                            eval += nearPromotionValue;
                        else
                        {
                            eval += (rank - minRank) * rankValue;
                        }
                        whitePawns++;
                        if (key >= 'A' && key <= 'D')
                            queensideNumber++;
                        else
                            kingsideNumber++;

                        if (IsPawnIsolated(files, key, 'P'))
                            eval -= isolatedPawnValue;

                        if (rank > 4)
                        {
                            bool enemyPawnPresent = false;
                            for(int j = i+1; j <= files[key].Length-minRank; ++j)
                            {
                                if (files[key][j] == 'p')
                                {
                                    enemyPawnPresent = true;
                                    break;
                                }
                            }
                            if (!enemyPawnPresent)
                            {
                                char leftRank = (char)(key - 1);
                                char rightRank = (char)(key + 1);
                                bool enemyPawnPresentL = true;
                                bool enemyPawnPresentR = true;
                                if (files.TryGetValue(leftRank, out char[] value))
                                {
                                    enemyPawnPresentL = false;
                                    for (int j = i + 1; j <= value.Length - minRank; ++j)
                                    {
                                        if (value[j] == 'p')
                                        {
                                            enemyPawnPresentL = true;
                                            break;
                                        }
                                    }
                                }
                                if (files.TryGetValue(rightRank, out value))
                                {
                                    enemyPawnPresentR = false;
                                    for (int j = i + 1; j <= value.Length - minRank; ++j)
                                    {
                                        if (value[j] == 'p')
                                        {
                                            enemyPawnPresentR = true;
                                            break;
                                        }
                                    }
                                }
                                if (!enemyPawnPresentL && !enemyPawnPresentR)
                                    eval += passedPawnValue;
                            }
                        }
                    }
                    else if (files[key][i] == 'p')
                    {
                        int rank = files[key].Length - i;
                        if (rank == nearPromitionRank)
                            eval -= nearPromotionValue;
                        else
                            eval -= (rank - minRank) * rankValue;

                        blackPawns++;
                        if (key >= 'A' && key <= 'D')
                            queensideNumber--;
                        else
                            kingsideNumber--;

                        if (IsPawnIsolated(files, key, 'p'))
                            eval += isolatedPawnValue;

                        if (rank > 4)
                        {
                            bool enemyPawnPresent = false;
                            for(int j = i-1; j >= minRank-1; --j)
                            {
                                if(files[key][j] == 'P')
                                {
                                    enemyPawnPresent = true;
                                    break;
                                }
                            }
                            if (!enemyPawnPresent)
                            {
                                char leftRank = (char)(key - 1);
                                char rightRank = (char)(key + 1);
                                bool enemyPawnPresentL = true;
                                bool enemyPawnPresentR = true;
                                if (files.TryGetValue(leftRank, out char[] value))
                                {
                                    enemyPawnPresentL = false;
                                    for (int j = i - 1; j >= minRank-1; --j)
                                    {
                                        if (value[j] == 'P')
                                        {
                                            enemyPawnPresentL = true;
                                            break;
                                        }
                                    }
                                }
                                if (files.TryGetValue(rightRank, out value))
                                {
                                    enemyPawnPresentR = false;
                                    for (int j = i - 1; j >= minRank - 1; --j)
                                    {
                                        if (value[j] == 'P')
                                        {
                                            enemyPawnPresentR = true;
                                            break;
                                        }
                                    }
                                }
                                if (!enemyPawnPresentL && !enemyPawnPresentR)
                                    eval -= passedPawnValue;
                            }
                        }
                    }
                }

                if (whitePawns > 1)
                    eval -= doubledPawnsValue * (whitePawns - 1);
                if (blackPawns > 1)
                    eval += doubledPawnsValue * (blackPawns - 1);

            }
            eval += kingsideNumber * pawnNumberAdvantageValue;
            eval += queensideNumber * pawnNumberAdvantageValue;
            return eval;
        }
        private static bool IsPawnIsolated(Dictionary<char, char[]> files, char pawnFile, char pawn)
        {
            char[] filesToCheck = new[] { (char)(pawnFile - 1), (char)(pawnFile + 1) };
            bool notIsolated = false;
            foreach (var file in filesToCheck)
            {
                if (files.TryGetValue(file, out char[] values))
                {
                    for (int i = 0; i < values.Length; ++i)
                    {
                        if (values[i] == pawn)
                        {
                            notIsolated = true;
                            break;
                        }
                    }
                }
            }
            return notIsolated;
        }
    }  
}
