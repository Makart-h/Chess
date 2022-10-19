using Chess.AI;
using Chess.Board;
using Chess.Data;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Positions;

internal sealed class Position : IPieceOwner
{
    public string ShortFEN { get; private set; }
    public King White { get; private set; }
    public King Black { get; private set; }
    public Team ActiveTeam { get; private set; }
    public int HalfMoves { get; init; }
    public Square? EnPassant { get; private set; }
    public bool Check { get; private set; }
    public Piece[] Pieces { get; init; }
    public Dictionary<int, int> OccuredPositions { get; init; }
    public List<Move> NextMoves { get; init; }
    public string MovePlayed { get; init; }
    public GameResult Result { get; set; } 
    public int Hash { get; private set; }
    private static readonly object s_locker = new();
    private Piece enPassantPiece;
    private Position(Piece[] pieces, Team activeTeam, in Move move, int halfMoves, Dictionary<int, int> occuredPositions)
    {
        Pieces = new Piece[pieces.Length];
        lock (s_locker)
        {
            OccuredPositions = new(occuredPositions);
        }
        NextMoves = new();
        ActiveTeam = activeTeam;
        HalfMoves = halfMoves;
        MovePlayed = move.Former.ToString() + ((int)move.Description).ToString() + move.Latter.ToString();
        CopyPieces(pieces);
        ApplyMove(in move);
        Update();
    }
    private Position(Position other, in Move move)
        : this(other.Pieces, other.ActiveTeam, in move, other.HalfMoves, other.OccuredPositions) { }
    private Position(Chessboard board, Team activeTeam, in Move move, Dictionary<int, int> occuredPositions, int halfMoves = 0)
        : this(board.Pieces, activeTeam, in move, halfMoves, occuredPositions) { }
    public static Task<Position> CreateAsync(Chessboard board, Team activeTeam, Move move, Dictionary<int, int> occuredPositions, CancellationToken token, int halfMoves = 0)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            Position position = new(board, activeTeam, move, occuredPositions, halfMoves);
            return position;
        });
    }
    public static Task<Position> CreateAsync(Position other, Move move, CancellationToken token)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            Position position = new(other, move);
            return position;
        });
    }
    private void CopyPieces(Piece[] other)
    {
        Piece pieceToCopy;
        Piece createdPiece;
        for(int i = 0; i < other.Length; ++i)
        {
            pieceToCopy = other[i];          
            if (pieceToCopy != null)
            {               
                createdPiece = PieceFactory.CopyAPiece(pieceToCopy, this, true);
                Pieces[i] = createdPiece;
                if (createdPiece.Moveset == Movesets.King)
                {
                    if (createdPiece.Team == Team.White)
                        White = (King)createdPiece;
                    else
                        Black = (King)createdPiece;
                }
            }
        }
    }
    public King GetKing(Team team)
    {
        return team switch
        {
            Team.White => White,
            Team.Black => Black,
            _ => null
        };
    }
    private void ApplyMove(in Move move)
    {
        int formerIndex = move.Former.Index;
        int latterIndex = move.Latter.Index;
        Pieces[latterIndex] = Pieces[formerIndex];
        Pieces[formerIndex] = null;
        if (move.Description == MoveType.EnPassant)
        {
            Square enPassant = new(move.Latter.Letter, move.Former.Digit);
            Pieces[enPassant.Index] = null;
        }
        else if (move.Description == MoveType.CastlesKingside || move.Description == MoveType.CastlesQueenside)
        {
            int direction = move.Former.Letter > move.Latter.Letter ? 1 : -1;
            Square originalRookPosition = King.GetCastlingRookSquare(move.Description, Pieces[latterIndex].Team);
            int originalRookPositionIndex = originalRookPosition.Index;
            Square newRookPosition = new(move.Latter, (direction, 0));
            Move rookMove = new(Pieces[originalRookPositionIndex].Square, newRookPosition, MoveType.ParticipatesInCastling);
            Pieces[originalRookPositionIndex].MovePiece(rookMove);
            Pieces[newRookPosition.Index] = Pieces[originalRookPositionIndex];
            Pieces[originalRookPositionIndex] = null;

        }
        Piece piece = Pieces[latterIndex];
        piece.MovePiece(in move);
        if (piece is Pawn { EnPassant: true } p)
        {
            EnPassant = new Square(piece.Square, (0, -p.Value));
            enPassantPiece = piece;
        }
        else
            EnPassant = new Square('p', 0);
    }
    private void Update()
    {
        ActiveTeam = ~ActiveTeam;
        if (White.Team == ActiveTeam)
            UpdateKings(White, Black);
        else
            UpdateKings(Black, White);

        AddNextMoves();
        FinishHashCreation();
    }
    private void FinishHashCreation()
    {
        Hash = unchecked(Hash * 7 + (int)White.CastlingRights);
        Hash = unchecked(Hash * 7 + (int)Black.CastlingRights);
        Hash = unchecked(Hash * 7 + (int)ActiveTeam);
        Hash = unchecked(Hash * 7 + EnPassant.Value.GetHashCode());

        if (OccuredPositions.TryGetValue(Hash, out var occurances))
            OccuredPositions[Hash] = occurances + 1;
        else
            OccuredPositions[Hash] = 1;
    }
    private void AddNextMoves()
    {
        Hash = 5;
        int sign;
        int whiteKingIndex = White.Square.Index;
        int blackKingIndex = Black.Square.Index;
        for(int i = 0; i < Pieces.Length; ++i)
        {
            Piece piece = Pieces[i];
            if (i == whiteKingIndex || i == blackKingIndex)
            {
                sign = piece.Team == Team.White ? 1 : -1;
                Hash = unchecked(Hash * 7 + (int)piece.Moveset * sign * (i + 1));
                continue;
            }           
            if (piece != null)
            {
                sign = piece.Team == Team.White ? 1 : -1;
                Hash = unchecked(Hash * 7 + (int)piece.Moveset * sign * (i + 1));
                piece.Update();
                if (piece == enPassantPiece)
                    ((Pawn)piece).EnPassant = true;
                if (piece.Team == ActiveTeam)
                {
                    foreach (Move move in piece.Moves)
                    {
                        if (move.Description != MoveType.Defends)
                            NextMoves.Add(move);
                    }
                }
            }
        }
    }
    private void UpdateKings(King activeKing, King standbyKing)
    {
        activeKing.Update();
        standbyKing.CheckPossibleMoves();
        foreach(Move move in activeKing.Moves)
        {
            if(move.Description != MoveType.Defends)
                NextMoves.Add(move);
        }
        if (activeKing.Threatened)
            Check = true;
    }
    public Piece GetPiece(Square square) => Pieces[square.Index];
    public Team GetTeamOnSquare(Square square) => Pieces[square.Index] == null ? Team.Empty : Pieces[square.Index].Team;
    public void OnPromotion(Piece piece)
    {
        PieceType type = PieceType.Queen;
        Square square = piece.Square;
        Team team = piece.Team;
        Pieces[square.Index] = null;
        try
        {
            Piece newPiece = PieceFactory.CreateAPiece(type, square, team);
            newPiece.Owner = piece.Owner;
            Pieces[square.Index] = newPiece;
        }
        catch (NotImplementedException e)
        {
            using var file = new FileStream("exceptions.txt", FileMode.Append, FileAccess.Write, FileShare.Write);
            using var writer = new StreamWriter(file);
            writer.WriteLine($"{DateTime.Now} - {e.Message} {e.StackTrace}");
            writer.Flush();
            throw;
        }
    }
}
