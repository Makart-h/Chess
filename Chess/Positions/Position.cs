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
    public Dictionary<string, int> OccuredPositions { get; init; }
    public List<Move> NextMoves { get; init; }
    public string MovePlayed { get; init; }
    public GameResult Result { get; set; } 
    private static readonly object s_locker = new object();
    private Position(Dictionary<Square, Piece> pieces, Team activeTeam, Move move, int halfMoves, Dictionary<string, int> occuredPositions)
    {
        Pieces = new Dictionary<Square, Piece>(Chessboard.NumberOfSquares * Chessboard.NumberOfSquares);
        lock (s_locker)
        {
            OccuredPositions = new(occuredPositions);
        }
        NextMoves = new();
        ActiveTeam = activeTeam;
        HalfMoves = halfMoves;
        Check = false;
        MovePlayed = $"{move.Former}{move.Description}{move.Latter}";
        CopyDictionary(pieces);
        ApplyMove(move);
        Update();
    }
    private Position(Position other, Move move)
        : this(other.Pieces, other.ActiveTeam, move, other.HalfMoves, other.OccuredPositions) { }
    private Position(Chessboard board, Team activeTeam, Move move, Dictionary<string, int> occuredPositions, int halfMoves = 0)
        : this(board.Pieces, activeTeam, move, halfMoves, occuredPositions) { }
    public static Task<Position> CreateAsync(Chessboard board, Team activeTeam, Move move, Dictionary<string, int> occuredPostions, CancellationToken token, int halfMoves = 0)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            Position position = new(board, activeTeam, move, occuredPostions, halfMoves);
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
            if (move.Description == 'p')
            {
            Square enPassant = new(move.Latter.Letter, move.Former.Digit);
            Pieces[enPassant.Index] = null;
            }
            else if (move.Description == 'k' || move.Description == 'q')
            {
                int direction = move.Former.Letter > move.Latter.Letter ? 1 : -1;
            Square originalRookPosition = King.GetCastlingRookSquare(move.Description, Pieces[latterIndex].Team);
            int originalRookPositionIndex = originalRookPosition.Index;
            Square newRookPosition = new(move.Latter, (direction, 0));
            Move rookMove = new(Pieces[originalRookPositionIndex].Square, newRookPosition, 'c');
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
        PrepareFEN();
    }
    private void PrepareFEN()
    {
        ShortFEN = FENParser.ToShortFenString(Pieces, White.CastlingRights, Black.CastlingRights, ActiveTeam);
        string enPassantSquare = ShortFEN[(ShortFEN.LastIndexOf(' ') + 1)..];
        EnPassant = enPassantSquare == "-" ? null : new Square(enPassantSquare);

        if (OccuredPositions.TryGetValue(ShortFEN, out var occurances))
            OccuredPositions[ShortFEN] = occurances + 1;
        else
            OccuredPositions[ShortFEN] = 1;
    }
    private void AddNextMoves()
    {
        foreach (var piece in Pieces.Values)
        {
            if (piece is King)
                continue;
            else if (piece != null && piece.Team == ActiveTeam)
            {
                piece.Update();
                if (piece.Moves.Count > 0)
                    NextMoves.AddRange(piece.Moves);
            }
        }
    }
    private void UpdateKings(King activeKing, King standbyKing)
    {
        activeKing.Update();
        standbyKing.CheckCastlingMoves();
        if (activeKing.Moves.Count > 0)
        {
            NextMoves.AddRange(activeKing.Moves);
        }
        if (activeKing.Threatened)
            Check = true;
    }
    public void PrepareForEvaluation()
    {
        if (ActiveTeam == Team.White)
            Black.FindAllThreats();
        else
            White.FindAllThreats();
    }
    public bool TryGetPiece(Square square, out Piece piece) => Pieces.TryGetValue(square, out piece) && piece != null;
    public Team GetTeamOnSquare(Square square) => Pieces[square] == null ? Team.Empty : Pieces[square].Team;
    public bool ArePiecesFacingEachOther(Piece first, Piece second)
    {
        (int letter, int digit) iterator;
        if (first.Square.Letter == second.Square.Letter)
        {
            iterator = (0, first.Square.Digit > second.Square.Digit ? -1 : 1);
        }
        else if (first.Square.Digit == second.Square.Digit)
        {
            iterator = (first.Square.Letter > second.Square.Letter ? -1 : 1, 0);
        }
        else if (Math.Abs(first.Square.Digit - second.Square.Digit) == Math.Abs(first.Square.Letter - second.Square.Letter))
        {
            int directionLetter = first.Square.Letter > second.Square.Letter ? -1 : 1;
            int directionDigit = first.Square.Digit > second.Square.Digit ? -1 : 1;
            iterator = (directionLetter, directionDigit);
        }
        else
            return false;

        Square square = first.Square;
        do
        {
            square.Transform(iterator);
            if (TryGetPiece(square, out Piece pieceOnTheWay))
                return pieceOnTheWay == second;
        }
        while (Square.Validate(square));

        return false;
    }
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
