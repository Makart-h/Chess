using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Data;
using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Data;
using Chess.Clock;

namespace Chess.AI
{
    internal static class Arbiter
    {
        
        private static readonly int _maxHalfMoves;
        private static int _halfMoves;
        public static Dictionary<string, int> OccuredPositions { get; private set; }
        public static EventHandler<GameResultEventArgs> GameConcluded;
        static Arbiter()
        {
            _maxHalfMoves = 50;
            _halfMoves = 0;
            OccuredPositions = new Dictionary<string, int>();
            Controller.MoveMade += OnMoveMade;
        }
        public static void Initilize(string fen, int halfMoves)
        {
            Arbiter._halfMoves = halfMoves;
            int index = fen.LastIndexOf(' ');
            fen = fen[..index];
            index = fen.LastIndexOf(' ');
            fen = fen[..index];
            OccuredPositions[fen] = 1;
        }
        public static string ExplainGameResult(GameResult result)
        {
            return result switch
            {
                GameResult.Black => "checkmate",
                GameResult.White => ExplainGameResult(GameResult.Black),
                GameResult.Stalemate => "stalemate",
                GameResult.Draw => "insufficient material",
                GameResult.ThreefoldRepetition => "threefold repetition claim",
                GameResult.FivefoldRepetition => "fivefold repetition",
                GameResult.DrawByAgreement => "both parties agreement",
                GameResult.HalfMoves => "reaching 50 half-moves",
                GameResult.InProgress => "game is in progress",
                _ => throw new NotImplementedException()
            };
        }
        public static GameResult AnalyzeBoard(Dictionary<Square, Piece> piecesOnTheBoard, Team toMove, int halfMoves, Dictionary<string, int> occuredPositions)
        {
            var notNullPieces = (from piece in piecesOnTheBoard.Values
                                 where piece != null
                                 select piece).ToArray();

            var whitePieces = (from piece in notNullPieces
                              where piece.Team == Team.White
                              select piece).ToArray();
            var whiteKing = (from piece in whitePieces
                             where piece is King
                            select piece as King).Single();

            var blackPieces = (from piece in notNullPieces
                              where piece.Team == Team.Black
                              select piece).ToArray();
            var blackKing = (from piece in blackPieces
                             where piece is King
                            select piece as King).Single();

            if (toMove == Team.White)
            {
                if (whiteKing.Owner is Controller c)
                    c.Update();
            }
            else
            {
                if (blackKing.Owner is Controller c)
                    c.Update();
            }

            var result = CheckForRepetition(piecesOnTheBoard, toMove, whiteKing.CastlingRights, blackKing.CastlingRights, occuredPositions);
            if (result != GameResult.InProgress)
                return result;

            result = CheckMateOrStalemate(toMove, whitePieces, blackPieces, whiteKing, blackKing);
            if (result != GameResult.InProgress)
                return result;

            if (halfMoves == _maxHalfMoves)
                return GameResult.HalfMoves;

            result = CheckMaterial(whitePieces, blackPieces);
            if (result != GameResult.InProgress)
                return result;

            return GameResult.InProgress;
        }
        private static GameResult CheckForRepetition(Dictionary<Square, Piece> pieces, Team toMove, CastlingRights white, CastlingRights black, Dictionary<string, int> occuredPositions)
        {
            string shortFen = FENParser.ToShortFenString(pieces, white, black, toMove);
            if (occuredPositions.TryGetValue(shortFen, out int value))
            {
                occuredPositions[shortFen] = value + 1;
                if (occuredPositions[shortFen] == 3)
                    return GameResult.ThreefoldRepetition;
                else if (occuredPositions[shortFen] == 5)
                    return GameResult.FivefoldRepetition;
            }
            else
                occuredPositions[shortFen] = 1;

            return GameResult.InProgress;
        }
        private static GameResult CheckMateOrStalemate(Team toMove, Piece[] whitePieces, Piece[] blackPieces, King whiteKing, King blackKing)
        {
            if (toMove == Team.White)
            {
                int legalMoves = (from piece in whitePieces
                                  select piece.Moves.Count).Sum();
                if (legalMoves == 0)
                    return whiteKing.Threatened ? GameResult.Black : GameResult.Stalemate;
            }
            else
            {
                int legalMoves = (from piece in blackPieces
                                  select piece.Moves.Count).Sum();
                if (legalMoves == 0)
                    return blackKing.Threatened ? GameResult.White : GameResult.Stalemate;
            }
            return GameResult.InProgress;
        }
        private static GameResult CheckMaterial(Piece[] whitePieces, Piece[] blackPieces)
        {
            Array.Sort(whitePieces);
            Array.Sort(blackPieces);

            // King vs king is a draw.
            if (whitePieces.Length == 1 && blackPieces.Length == 1)
                return GameResult.Draw;
            else if (whitePieces.Length == 1 && blackPieces.Length == 2)
            {
                var secondPiece = blackPieces[^1];
                // King vs king + bishop/knight is a draw.
                if (secondPiece is Knight || secondPiece is Bishop)
                    return GameResult.Draw;
            }
            else if (blackPieces.Length == 1 && whitePieces.Length == 2)
            {
                var secondPiece = whitePieces[^1];
                // King vs king + bishop/knight is a draw.
                if (secondPiece is Knight || secondPiece is Bishop)
                    return GameResult.Draw;
            }
            else if (whitePieces.Length == 2 && blackPieces.Length == 2)
            {
                var secondWhitePiece = whitePieces[^1];
                var secondBlackPiece = blackPieces[^1];
                // King + bishop vs king + bishop is a draw when both bishops are on the squares of the same color.
                if (secondWhitePiece is Bishop && secondBlackPiece is Bishop) 
                {
                    bool isWhiteOnLightSquare = 
                        ((secondWhitePiece.Square.Letter - 'A') % 2 != 0 && secondWhitePiece.Square.Digit % 2 != 0) ||
                        ((secondWhitePiece.Square.Letter - 'A') % 2 == 0 && secondWhitePiece.Square.Digit % 2 == 0);
                    bool isBlackOnLightSquare =
                        ((secondBlackPiece.Square.Letter - 'A') % 2 != 0 && secondBlackPiece.Square.Digit % 2 != 0) ||
                        ((secondBlackPiece.Square.Letter - 'A') % 2 == 0 && secondBlackPiece.Square.Digit % 2 == 0);

                    if (isWhiteOnLightSquare && isBlackOnLightSquare)
                        return GameResult.Draw;
                    else if (!isWhiteOnLightSquare && !isBlackOnLightSquare)
                        return GameResult.Draw;
                }
            }
            return GameResult.InProgress;
        }
        private static void OnGameConcluded(GameResultEventArgs e) => GameConcluded?.Invoke(null, e);
        private static void OnMoveMade(object sender, MoveMadeEventArgs e)
        {
            // Half move is any move that is not a caputre or a pawn advance.
            if (e.Piece is Pawn || e.Move.Description == 'x')
                _halfMoves = 0;
            else
                _halfMoves++;

            GameResult result = AnalyzeBoard(Chessboard.Instance.Pieces, ~e.Controller.Team, _halfMoves, OccuredPositions);
            if (result != GameResult.InProgress)
                OnGameConcluded(new GameResultEventArgs(result));
        }
    }
}
