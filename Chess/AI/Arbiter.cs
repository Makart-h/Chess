using Chess.Board;
using Chess.Clock;
using Chess.Data;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.AI;

internal static class Arbiter
{

    private static readonly int _maxHalfMoves;
    private static int _halfMoves;
    private static Team _teamToAnalyze;
    public static Dictionary<int, int> OccuredPositions { get; private set; }
    public static EventHandler<GameResultEventArgs> GameConcluded;
    static Arbiter()
    {
        _maxHalfMoves = 50;
        _halfMoves = 0;
        OccuredPositions = new Dictionary<int, int>();
        Controller.MoveMade += OnMoveMade;
        Chess.TurnEnded += OnTurnEnded;
    }
    public static void Initilize(FENObject fen)
    {
        _halfMoves = fen.HalfMoves;
        int hash = CalculateHash(fen);
        OccuredPositions[hash] = 1;
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
    public static GameResult AnalyzeBoard(Piece[] piecesOnTheBoard, Team toMove, int halfMoves, Dictionary<int, int> occuredPositions, Controller currentController = null, int? hash = null)
    {
        if (halfMoves == _maxHalfMoves)
            return GameResult.HalfMoves;

        if(currentController != null)
            currentController.Update();

        King whiteKing = null;
        King blackKing = null;
        List<Piece> whitePieces = new();
        List<Piece> blackPieces = new();
        int whiteMoves = 0;
        int blackMoves = 0;
        int kingsToFind = 2;
        for(int i = 0; i < piecesOnTheBoard.Length; ++i)
        {
            Piece piece = piecesOnTheBoard[i];
            if (piece != null)
            {
                if (piece.Team == Team.White)
                {
                    whitePieces.Add(piece);
                    whiteMoves += piece.Moves.Count;
                    if (kingsToFind > 0 && piece is King k)
                        whiteKing = k;
                }
                else
                {
                    blackPieces.Add(piece);
                    blackMoves += piece.Moves.Count;
                    if (kingsToFind > 0 && piece is King k)
                        blackKing = k;
                }
            }
        }

        int h = hash ?? CalculateHash(piecesOnTheBoard, whiteKing.CastlingRights, blackKing.CastlingRights, toMove);
        var result = CheckForRepetition(h, occuredPositions);
        if (result != GameResult.InProgress)
          return result;

        if (toMove == Team.White)
            result = CheckMateOrStalemate(toMove, whiteKing, whiteMoves);
        else
            result = CheckMateOrStalemate(toMove, blackKing, blackMoves);

        if (result != GameResult.InProgress)
            return result;

        result = CheckMaterial(whitePieces, blackPieces);
        if (result != GameResult.InProgress)
            return result;

        return GameResult.InProgress;
    }
    private static GameResult CheckForRepetition(int hash, Dictionary<int, int> occuredPositions)
    {
        if (occuredPositions.TryGetValue(hash, out int value))
        {
            occuredPositions[hash] = value + 1;
            if (occuredPositions[hash] == 3)
                return GameResult.ThreefoldRepetition;
            else if (occuredPositions[hash] == 5)
                return GameResult.FivefoldRepetition;
        }
        else
            occuredPositions[hash] = 1;

        return GameResult.InProgress;
    }
    private static GameResult CheckMateOrStalemate(Team toMove, King king, int legalMoves)
    {
        GameResult potentialWinner = toMove == Team.White ? GameResult.Black : GameResult.White;

        if (legalMoves == 0)
            return king.Threatened ? potentialWinner : GameResult.Stalemate;

        return GameResult.InProgress;
    }
    private static GameResult CheckMaterial(List<Piece> whitePieces, List<Piece> blackPieces)
    {      
        // King vs king is a draw.
        if (whitePieces.Count == 1 && blackPieces.Count == 1)
            return GameResult.Draw;
        else if (whitePieces.Count == 1 && blackPieces.Count == 2)
        {
            blackPieces.Sort();
            var secondPiece = blackPieces[0];
            // King vs king + bishop/knight is a draw.
            if (secondPiece.Value == -3)
                return GameResult.Draw;
        }
        else if (blackPieces.Count == 1 && whitePieces.Count == 2)
        {
            whitePieces.Sort();
            var secondPiece = whitePieces[0];
            // King vs king + bishop/knight is a draw.
            if (secondPiece.Value == 3)
                return GameResult.Draw;
        }
        else if (whitePieces.Count == 2 && blackPieces.Count == 2)
        {
            whitePieces.Sort();
            blackPieces.Sort();
            var secondWhitePiece = whitePieces[0];
            var secondBlackPiece = blackPieces[0];
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
    private static int CalculateHash(Piece[] piecesOnTheBoard, CastlingRights white, CastlingRights black, Team toMove)
    {
        int hash = 5;
        Square enPassant = new('p', 0);
        for (int i = 0; i < piecesOnTheBoard.Length; i++)
        {
            Piece piece = piecesOnTheBoard[i];
            if (piece != null)
            {
                int sign = piece.Team == Team.White ? 1 : -1;
                hash = unchecked(hash * 7 + (int)piece.Moveset * sign * (i + 1));
                if (piece is Pawn { EnPassant: true })
                    enPassant = new Square(piece.Square, (0, -piece.Value));
            }
        }
        hash = unchecked(hash * 7 + (int)white);
        hash = unchecked(hash * 7 + (int)black);
        hash = unchecked(hash * 7 + (int)toMove);
        hash = unchecked(hash * 7 + enPassant.GetHashCode());
        return hash;
    }
    private static int CalculateHash(FENObject fen)
    {
        int hash = 5;
        Square enPassant = fen.EnPassantSquare ?? (new('p', 0));
        foreach(Piece piece in fen.AllPieces)
        {
            int sign = piece.Team == Team.White ? 1 : -1;
            hash = unchecked(hash * 7 + (int)piece.Moveset * sign * (piece.Square.Index + 1));
        }
        hash = unchecked(hash * 7 + (int)fen.WhiteCastling);
        hash = unchecked(hash * 7 + (int)fen.BlackCastling);
        hash = unchecked(hash * 7 + (int)fen.TeamToMove);
        hash = unchecked(hash * 7 + enPassant.GetHashCode());
        return hash;
    }
    private static void OnGameConcluded(GameResultEventArgs e) => GameConcluded?.Invoke(null, e);
    private static void OnMoveMade(object sender, MoveMadeEventArgs e)
    {
        // Half move is any move that is not a caputre or a pawn advance.
        if (e.Piece is Pawn || e.Move.Description == MoveType.Takes)
            _halfMoves = 0;
        else
            _halfMoves++;

        _teamToAnalyze = ~e.Controller.Team;  
    }
    private static void OnTurnEnded(object sender, EventArgs e)
    {
        GameResult result = AnalyzeBoard(Chessboard.Instance.Pieces, _teamToAnalyze, _halfMoves, OccuredPositions, ChessClock.InactiveController);
        if (result != GameResult.InProgress)
            OnGameConcluded(new GameResultEventArgs(result));
    }
}
