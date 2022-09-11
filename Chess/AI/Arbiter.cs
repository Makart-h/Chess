using System;
using System.Collections.Generic;
using System.Text;
using Chess.Board;
using Chess.Pieces;
using System.Linq;
using Chess.Data;
using Chess.Clock;

namespace Chess.AI
{
    internal static class Arbiter
    {
        
        private static readonly int maxHalfMoves;
        private static int halfMoves;
        private static Dictionary<string, int> occuredPositions;
        public static EventHandler<GameResultEventArgs> GameConcluded;
        static Arbiter()
        {
            maxHalfMoves = 50;
            halfMoves = 0;
            occuredPositions = new Dictionary<string, int>();
            Controller.MoveMade += OnMoveChosen;
        }
        public static void Initilize(string fen, int halfMoves)
        {
            Arbiter.halfMoves = halfMoves;
            int index = fen.LastIndexOf(' ');
            fen = fen[..index];
            index = fen.LastIndexOf(' ');
            fen = fen[..index];
            occuredPositions[fen] = 1;
        }
        private static GameResult AnalyzeBoard(Dictionary<Square, Piece> piecesOnTheBoard, Team toMove, int halfMoves, ref Dictionary<string, int> occuredPositions)
        {
            var notNullPieces = (from piece in piecesOnTheBoard.Values
                                 where piece != null select piece).ToArray();

            var whitePieces = (from piece in notNullPieces
                              where piece.Team == Team.White
                              select piece).ToArray();
            var whiteKing = (from piece in whitePieces where piece is King
                            select piece as King).Single();

            var blackPieces = (from piece in notNullPieces
                              where piece.Team == Team.Black
                              select piece).ToArray();
            var blackKing = (from piece in blackPieces where piece is King
                            select piece as King).Single();

            if(toMove == Team.White)
            {
                if (whiteKing.Owner is Controller c)
                    c.Update();
            }
            else
            {
                if (blackKing.Owner is Controller c)
                    c.Update();
            }

            var result = CheckForRepetition(piecesOnTheBoard, toMove, whiteKing.CastlingRights, blackKing.CastlingRights, ref occuredPositions);
            if (result != GameResult.InProgress)
                return result;

            result = CheckMateOrStalemate(toMove, whitePieces, blackPieces, whiteKing, blackKing);
            if (result != GameResult.InProgress)
                return result;

            if(halfMoves == maxHalfMoves)
                return GameResult.Draw;

            result = CheckMaterial(whitePieces, blackPieces);
            if (result != GameResult.InProgress)
                return result;

            return GameResult.InProgress;
        }
        private static GameResult CheckForRepetition(Dictionary<Square, Piece> pieces, Team toMove, CastlingRights white, CastlingRights black, ref Dictionary<string, int> occuredPositions)
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

            if (whitePieces.Length == 1 && blackPieces.Length == 1) //king vs king is a draw
                return GameResult.Draw;
            else if (whitePieces.Length == 1 && blackPieces.Length == 2)
            {
                var secondPiece = blackPieces[^1];
                if (secondPiece is Knight || secondPiece is Bishop) //king vs king + bishop/knight is a draw
                    return GameResult.Draw;
            }
            else if(blackPieces.Length == 1 && whitePieces.Length == 2)
            {
                var secondPiece = whitePieces[^1];
                if (secondPiece is Knight || secondPiece is Bishop) //king vs king + bishop/knight is a draw
                    return GameResult.Draw;
            }
            else if(whitePieces.Length == 2 && blackPieces.Length == 2)
            {
                var secondWhitePiece = whitePieces[^1];
                var secondBlackPiece = blackPieces[^1];
                if(secondWhitePiece is Bishop && secondBlackPiece is Bishop) //king + bishop vs king + bishop is a draw when both bishops are on the squares of the same color
                {
                    bool isWhiteOnLightSquare = 
                        ((secondWhitePiece.Square.Number.letter - 'A') % 2 != 0 && secondWhitePiece.Square.Number.digit % 2 != 0) ||
                        ((secondWhitePiece.Square.Number.letter - 'A') % 2 == 0 && secondWhitePiece.Square.Number.digit % 2 == 0);
                    bool isBlackOnLightSquare =
                        ((secondBlackPiece.Square.Number.letter - 'A') % 2 != 0 && secondBlackPiece.Square.Number.digit % 2 != 0) ||
                        ((secondBlackPiece.Square.Number.letter - 'A') % 2 == 0 && secondBlackPiece.Square.Number.digit % 2 == 0);

                    if (isWhiteOnLightSquare && isBlackOnLightSquare)
                        return GameResult.Draw;
                    else if (!isWhiteOnLightSquare && !isBlackOnLightSquare)
                        return GameResult.Draw;
                }
            }
            return GameResult.InProgress;
        }
        private static void OnGameConcluded(GameResultEventArgs args) => GameConcluded?.Invoke(null, args);
        private static void OnMoveChosen(object sender, MoveMadeEventArgs args)
        {
            if (args.Piece is Pawn || args.Move.Description == 'x') //halfmove is any move that is not a caputre or a pawn advance
                halfMoves = 0;
            else
                halfMoves++;

            GameResult result = AnalyzeBoard(Chessboard.Instance.Pieces, ~args.Controller.Team, halfMoves, ref occuredPositions);
            if (result != GameResult.InProgress)
                OnGameConcluded(new GameResultEventArgs(result));
        }
    }
}
